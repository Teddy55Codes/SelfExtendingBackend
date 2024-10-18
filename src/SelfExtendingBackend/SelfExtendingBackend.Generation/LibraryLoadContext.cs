using System.Reflection;
using System.Runtime.Loader;

namespace SelfExtendingBackend.Generation;

public class LibraryLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public LibraryLoadContext(string pluginPath) 
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName)!;
        return assemblyPath != null! ? LoadFromAssemblyPath(assemblyPath) : null!;
    }
}