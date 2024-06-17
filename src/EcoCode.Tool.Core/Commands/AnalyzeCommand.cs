namespace EcoCode.Tool.Core.Commands;

internal sealed class AnalyzeCommand(Tool.Workspace workspace) : AsyncCommand<AnalyzeSettings>
{
    public override ValidationResult Validate(CommandContext context, AnalyzeSettings settings)
    {
        if (!File.Exists(settings.Source))
            return ValidationResult.Error($"The source file {settings.Source} does not exist.");

        if (Path.GetDirectoryName(settings.Output) is { } outputDir)
        {
            try
            {
                _ = Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"The output directory {outputDir} cannot be opened or created: {ex.Message}");
            }
        }

        return base.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings)
    {
        IAnalysisReport report;

        if (settings.SourceType is SourceType.Solution)
        {
            if (await AnalyzeService.OpenSolutionAsync(workspace, settings.Source) is not { } solution) return 1;

            var analizers = settings.GetActiveAnalyzers();
            report = AnalysisReport.Create(settings.OutputType);
            foreach (var project in solution.Projects)
                await AnalyzeService.AnalyzeProject(project, analizers, report);
        }
        else // options.SourceType is SourceType.Project
        {
            if (await AnalyzeService.OpenProjectAsync(workspace, settings.Source) is not { } project) return 1;

            var analizers = settings.GetActiveAnalyzers();
            report = AnalysisReport.Create(settings.OutputType);
            await AnalyzeService.AnalyzeProject(project, analizers, report);
        }

        report.WriteToFile(settings.Output!);
        return 0;
    }
}
