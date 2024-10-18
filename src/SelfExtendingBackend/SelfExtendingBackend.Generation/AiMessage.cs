using NuGet.Versioning;

namespace SelfExtendingBackend.Generation;

public record AiMessage(string Name, List<(string packageId, NuGetVersion version)> Dependencies, string Code);