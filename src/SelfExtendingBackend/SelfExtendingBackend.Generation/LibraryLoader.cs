using System.Reflection;
using System.Composition.Hosting;
using SelfExtendingBackend.Contract;

namespace SelfExtendingBackend.Generation;

public class LibraryLoader
{
    private readonly string _libraryName;
    private readonly string _libraryPath;
    
    public LibraryLoader(string libraryName, string libraryPath)
    {
        _libraryName = libraryName;
        _libraryPath =  Path.Combine(libraryPath, "bin/Debug/net8.0");
    }

    public IEndpoint LoadLibrary()
    {
        var libraryLoadContext = new LibraryLoadContext(Path.Combine(_libraryPath, $"{_libraryName}.dll"));
        Assembly libraryAssembly = libraryLoadContext.LoadFromAssemblyName(new AssemblyName(_libraryName));
        
        var configuration = new ContainerConfiguration().WithAssembly(libraryAssembly);
        using var container = configuration.CreateContainer();
        return container.GetExport<IEndpoint>();
    }
}
