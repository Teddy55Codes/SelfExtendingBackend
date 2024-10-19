using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Routing.Patterns;
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


var app = builder.Build();

// Enable CORS globally
app.UseCors("AllowAll");

// Placeholder to store the new endpoint route
var dynamicEndpoints = new RouteEndpointBuilder(
    context => Task.CompletedTask, RoutePatternFactory.Parse("/"), 0);

app.UseRouting();

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

        // call method from sven
        var endpointGenerator = new EndpointGenerator();
        Result<IEndpoint> result = endpointGenerator.GenerateEndpoint(inputString);
        if (result.IsSuccess)
        {
            // Register a dynamic endpoint on-the-fly
            dynamicEndpoints = new RouteEndpointBuilder(
                async context =>
                {
                    using var reader = new StreamReader(context.Request.Body);
                    var requestBodyString = await reader.ReadToEndAsync();
                    await context.Response.BodyWriter.WriteAsync(await result.Value
                        .Request(requestBodyString).ReadAsByteArrayAsync());
                },
                RoutePatternFactory.Parse(result.Value.Url),
                0
            );
        }
    });
});



// Custom middleware to handle dynamic routing
app.Use(async (context, next) =>
{
    var endpoint = dynamicEndpoints.Build();
    var routeEndpoint = endpoint as RouteEndpoint;

    // If the request matches the dynamic endpoint path, execute it
    if (context.Request.Path == routeEndpoint?.RoutePattern.RawText)
    {
        await routeEndpoint?.RequestDelegate(context);
    }
    else
    {
        await next();
    }
});

app.Run();

public class InputData
{
    public string Value { get; set; } // The value sent in the POST request
}