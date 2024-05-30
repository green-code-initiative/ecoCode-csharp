using Microsoft.CodeAnalysis.MSBuild;

const string targetPath = @"C:\ecocode\ecoCode-csharp-test-project\ecoCode-csharp-test-project.sln";

using (var workspace = MSBuildWorkspace.Create())
{
    var sln = await workspace.OpenSolutionAsync(targetPath).ConfigureAwait(false);
    foreach (var project in sln.Projects)
        Console.WriteLine(project.AssemblyName);
}

Console.ReadKey();
