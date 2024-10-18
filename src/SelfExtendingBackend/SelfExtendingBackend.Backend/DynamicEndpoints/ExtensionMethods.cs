namespace SelfExtendingBackend.Backend.DynamicEndpoints;

public static class ExtensionMethods
{
    public static void AddMyEndpoints(this IServiceCollection services)
    {
        services.AddSingleton<MyEndpointDataSource>();
    }
    
    public static void UseMyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.ServiceProvider.GetService<MyEndpointDataSource>();

        if (dataSource is null)
        {
            throw new Exception("Did you forget to call Services.AddMyEndpoints()?");
        }

        endpoints.DataSources.Add(dataSource);
    }
}