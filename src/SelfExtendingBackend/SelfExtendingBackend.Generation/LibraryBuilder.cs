using System.Diagnostics;
using System.Xml;
using FluentResults;
using NuGet.Versioning;
using SelfExtendingBackend.Contract;

namespace SelfExtendingBackend.Generation;

public class LibraryBuilder
{
    private readonly AiMessage _aiMessage;

    public LibraryBuilder(AiMessage aiMessage)
    {
        _aiMessage = aiMessage;
    }

    public Result BuildProject()
    {
        var csprojContent = AddPackageDependencies(_aiMessage.Dependencies);
        
        Directory.CreateDirectory(_aiMessage.Name);
        
        File.WriteAllText(Path.Combine(_aiMessage.Name, $"{_aiMessage.Name}.csproj"), csprojContent);
        File.WriteAllText(Path.Combine(_aiMessage.Name, $"{_aiMessage.Name}.cs"), _aiMessage.Code);
        
        Process dotNetCLI = new Process();
        dotNetCLI.StartInfo.FileName = "dotnet";
        dotNetCLI.StartInfo.RedirectStandardOutput = true;
        dotNetCLI.StartInfo.RedirectStandardError = true;
        dotNetCLI.StartInfo.UseShellExecute = false;
        dotNetCLI.StartInfo.Arguments = $"build {_aiMessage.Name}/{_aiMessage.Name}.csproj";
        
        dotNetCLI.Start();
        dotNetCLI.WaitForExit();

        Result res = Result.Ok();
        if (dotNetCLI.ExitCode != 0)
        {
            var error = dotNetCLI.StandardError.ReadToEnd();
            res = Result.Fail(error);
            Console.WriteLine($"error: {error}");
            Directory.Delete(_aiMessage.Name, true);
        }
        return res;
    }
    
    private string AddPackageDependencies(List<(string id, NuGetVersion version)> dependencies)
    {
        var doc = new XmlDocument();
        doc.Load("Resources/CsprojTemplate.xml");
        XmlNode root = doc.DocumentElement!;
        XmlNode itemGroupNode = root.SelectSingleNode("ItemGroup")!;

        foreach (var dependency in dependencies)
        {
            if (dependency.id == $"{nameof(SelfExtendingBackend)}.{nameof(Contract)}") continue;
            
            XmlElement projectReference = doc.CreateElement("PackageReference");
            projectReference.SetAttribute("Include", dependency.id);
            projectReference.SetAttribute("Version", dependency.version.ToString());
            itemGroupNode.AppendChild(projectReference);
        }

        return doc.InnerXml;
    }
}