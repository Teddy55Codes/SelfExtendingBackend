using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Patterns;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


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

        // Register a dynamic endpoint on-the-fly
        dynamicEndpoints = new RouteEndpointBuilder(
            async context =>
                await context.Response.WriteAsync(
                    $"This is the new dynamic endpoint! The stored value is: {inputString}"),
            RoutePatternFactory.Parse("/new-new-endpoint"),
            0
        );
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