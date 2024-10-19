namespace SelfExtendingBackend.Frontend.Models;

public class ProcessStep
{
    public string Name { get; set; }
    public string Status { get; set; } // "ready", "failed", "in progress", "waiting"
}

