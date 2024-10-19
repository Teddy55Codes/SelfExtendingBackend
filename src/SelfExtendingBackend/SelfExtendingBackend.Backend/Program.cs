using System.Text.Json;
using FluentResults;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing.Patterns;
using SelfExtendingBackend.Backend;
using SelfExtendingBackend.Contract;
using SelfExtendingBackend.Generation;

var options = new WebApplicationOptions
{
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory // Set to actual runtime folder in bin/Debug
};

var builder = WebApplication.CreateBuilder(options);

var executionPath = AppContext.BaseDirectory; // Use the actual runtime directory
builder.Host.UseContentRoot(executionPath); // Set ContentRoot to runtime folder

// Enable CORS to allow any origin, method, and headers.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

var app = builder.Build();

// Enable CORS globally
app.UseCors("AllowAll");

app.UseResponseCompression();

app.UseRouting();

// List to store all dynamically registered endpoints
var dynamicEndpointsList = new List<RouteEndpointBuilder>();

var customEndpointsList = new List<EntpointDTO>();

app.UseEndpoints(endpoints =>
{
    // First static endpoint: /new-endpoint (POST)
    endpoints.MapPost("/new-endpoint", async context =>
    {
        // Parse the input from the request body
        var requestBody = await JsonSerializer.DeserializeAsync<InputData>(context.Request.Body);

        if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.Value))
        {
            context.Response.StatusCode = 400; // Bad request
            await context.Response.WriteAsync("Invalid input. Please provide a valid 'Value'.");
            return;
        }

        // Store the input value in a variable
        var inputString = requestBody.Value;

        // Call method from sven
        var endpointGenerator = new EndpointGenerator();
        Result<(IEndpoint, AiMessage)> result = endpointGenerator.GenerateEndpoint(inputString);
        if (result.IsSuccess)
        {
            // Register a dynamic endpoint on-the-fly
            customEndpointsList.Add(new EntpointDTO()
            {
                ClassName = result.Value.Item2.Name,
                Code = result.Value.Item2.Code,
                Dependencies = result.Value.Item2.Dependencies.Select(t => t.packageId + ":" + t.version.ToString()).ToArray(),
                URL = result.Value.Item1.Url, 
                Promt = inputString
            });

            var dynamicEndpoint = new RouteEndpointBuilder(
                async context =>
                {
                    using var reader = new StreamReader(context.Request.Body);
                    var requestBodyString = await reader.ReadToEndAsync();
                    await context.Response.BodyWriter.WriteAsync(await result.Value.Item1
                        .Request(requestBodyString).ReadAsByteArrayAsync());
                },
                RoutePatternFactory.Parse(result.Value.Item1.Url),
                0
            );

            // Add the newly created endpoint to the list
            dynamicEndpointsList.Add(dynamicEndpoint);
        }
    });

    endpoints.MapGet("/get-all-endpoints", async context =>
    {
        await context.Response.WriteAsJsonAsync(customEndpointsList);
    });
});

// Custom middleware to handle dynamic routing
app.Use(async (context, next) =>
{
    // Loop through all dynamically created endpoints
    foreach (var dynamicEndpointBuilder in dynamicEndpointsList)
    {
        var endpoint = dynamicEndpointBuilder.Build();
        var routeEndpoint = endpoint as RouteEndpoint;

        // If the request matches the dynamic endpoint path, execute it
        if (context.Request.Path == routeEndpoint?.RoutePattern.RawText)
        {
            await routeEndpoint?.RequestDelegate(context);
            return; // Stop further processing if a matching endpoint is found
        }
    }

    await next(); // Continue to the next middleware if no dynamic endpoint matched
});

app.Run();

public class InputData
{
    public string Value { get; set; } // The value sent in the POST request
}
