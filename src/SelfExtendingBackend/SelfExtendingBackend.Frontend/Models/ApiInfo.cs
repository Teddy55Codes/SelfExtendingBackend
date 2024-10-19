namespace SelfExtendingBackend.Frontend.Models;

public class ApiInfo
{
    public string Name { get; set; }
    public string Route { get; set; }
    public string Description { get; set; }
    public string HttpMethod { get; set; } // e.g., GET, POST
}