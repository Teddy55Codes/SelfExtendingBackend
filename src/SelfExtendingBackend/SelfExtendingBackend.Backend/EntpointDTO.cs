using NuGet.Versioning;

namespace SelfExtendingBackend.Backend;

public class EntpointDTO
{
    public string ClassName { get; set; }
    
    public string Code { get; set; }
    
    public string[] Dependencies { get; set; }
    
    public string URL { get; set; }

    public string Promt { get; set; }
}