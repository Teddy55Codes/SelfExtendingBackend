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
        _libraryPath = libraryPath;
    }

    public void LoadLibrary()
    {
        var libraryLoadContext = new LibraryLoadContext(_libraryPath);
        Assembly libraryAssembly = libraryLoadContext.LoadFromAssemblyName(new AssemblyName(_libraryName));
        
        var configuration = new ContainerConfiguration().WithAssembly(libraryAssembly);
        using (var container = configuration.CreateContainer())
        {
            var library = container.GetExport<IEndpoint>();
            Console.WriteLine(library.Url); 
        }
    }
}
