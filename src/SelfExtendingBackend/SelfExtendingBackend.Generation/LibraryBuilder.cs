using System.Diagnostics;
using System.Xml;
using FluentResults;
using NuGet.Versioning;

namespace SelfExtendingBackend.Generation;

public class LibraryBuilder
{
    public XmlDocument _projectFile;

    public Result BuildProject(string libraryName, string codeContent)
    {
        Directory.CreateDirectory(libraryName);
        File.WriteAllText(Path.Combine(libraryName, $"{libraryName}.csproj"), _projectFile.InnerXml);
        File.WriteAllText(Path.Combine(libraryName, $"{libraryName}.cs"), codeContent);
        
        Process dotNetCLI = new Process();
        dotNetCLI.StartInfo.FileName = "dotnet";
        dotNetCLI.StartInfo.RedirectStandardOutput = true;
        dotNetCLI.StartInfo.RedirectStandardError = true;
        dotNetCLI.StartInfo.UseShellExecute = false;
        dotNetCLI.StartInfo.Arguments = $"Build {libraryName}/{libraryName}.csproj";
        
        dotNetCLI.Start();
        dotNetCLI.WaitForExit();
        
        var res = new Result();
        if (dotNetCLI.ExitCode != 0)
        {
            res.Errors.Add(new Error(dotNetCLI.StandardError.ReadToEnd()));
        }
        return res;
    }
    
    public void AddPackageDependencies(List<(string id, NuGetVersion version)> dependencies)
    {
        var doc = new XmlDocument();
        doc.Load("Resources/CsprojTemplate.xml");
        XmlNode root = doc.DocumentElement!;
        XmlNode itemGroupNode = root.SelectSingleNode("ItemGroup")!;

        foreach (var dependency in dependencies)
        {
            XmlElement projectReference = doc.CreateElement("PackageReference");
            projectReference.SetAttribute("Include", dependency.id);
            projectReference.SetAttribute("Version", dependency.version.ToString());
            itemGroupNode.AppendChild(projectReference);
        }

        _projectFile = doc;
    }
}