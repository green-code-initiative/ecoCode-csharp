using EcoCode.Tool.Library;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System.Linq;

if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
{
    Tool.WriteLine("No MSBuild instance was found, exiting.", "red");
    return 1;
}

Tool.WriteLine($"Using MSBuild found at {instance.MSBuildPath}.");
MSBuildLocator.RegisterInstance(instance);

using var workspace = MSBuildWorkspace.Create();
workspace.WorkspaceFailed += (sender, e) => Tool.WriteLine(e.Diagnostic.Message, "red");

var delegates = new Tool.Delegates(
    filePath => workspace.OpenSolutionAsync(filePath),
    filePath => workspace.OpenProjectAsync(filePath));

int errorCode = await Tool.RunAsync(delegates, args);

if (!Console.IsOutputRedirected) // Running in interactive mode
{
    Tool.WriteLine("Press a key to exit..");
    _ = Console.ReadKey();
}
return errorCode;
