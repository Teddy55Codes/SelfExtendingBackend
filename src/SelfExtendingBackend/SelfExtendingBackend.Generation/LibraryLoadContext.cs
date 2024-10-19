using System.Reflection;
using System.Runtime.Loader;

namespace SelfExtendingBackend.Generation;

public class LibraryLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public LibraryLoadContext(string libraryPath)
    {
        _resolver = new AssemblyDependencyResolver(libraryPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == $"{nameof(SelfExtendingBackend)}.{nameof(Contract)}") return null!;
        
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName)!;
        return assemblyPath != null! ? LoadFromAssemblyPath(assemblyPath) : null!;
    }
}