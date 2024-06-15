using EcoCode.Tool.Core;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
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

int errorCode = await Tool.RunAsync(args,
    openSolutionAsync: filePath => workspace.OpenSolutionAsync(filePath),
    openProjectAsync: filePath => workspace.OpenProjectAsync(filePath))
    .ConfigureAwait(false);

if (!Console.IsOutputRedirected) // Interactive mode
{
    Tool.WriteLine("Press a key to exit..");
    _ = Console.ReadKey();
}

return errorCode;
