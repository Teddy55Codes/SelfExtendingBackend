using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace SelfExtendingBackend.Backend.DynamicEndpoints;


public class MyEndpointDataSource : MutableEndpointDataSource
{
    public MyEndpointDataSource()
    {
        SetEndpoints(MakeEndpoints("myEndpoint"));
    }

    private IReadOnlyList<Endpoint> MakeEndpoints(string route)
        => new[]
        {
            // routes have to start with /
            CreateEndpoint($"/{route}", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            }),
            CreateEndpoint("/setEndpoint/{**route}", async context => {
                SetEndpoints(MakeEndpoints(context.Request.RouteValues["route"].ToString()));
            })
        };

    private static Endpoint CreateEndpoint(string pattern, RequestDelegate requestDelegate) =>
        new RouteEndpointBuilder(
                requestDelegate: requestDelegate,
                routePattern: RoutePatternFactory.Parse(pattern),
                order: 0)
            .Build();
}