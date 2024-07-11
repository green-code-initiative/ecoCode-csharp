using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace EcoCode.Tool.Common;

internal sealed class AdditionalFile(string path, string text) : AdditionalText
{
    public const string GlobalConfigFile = "EcoCode.globalconfig";

    public override string Path { get; } = path;

    public string Text { get; } = text;

    public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(Text);

    public static async Task<AdditionalFile> LoadGlobalConfigAsync()
    {
        string editorConfigPath = System.IO.Path.Combine(AppContext.BaseDirectory, GlobalConfigFile);
        if (!File.Exists(editorConfigPath))
            throw new FileNotFoundException($"Editor config file not found at {editorConfigPath}");

        string fileContent = await File.ReadAllTextAsync(editorConfigPath).ConfigureAwait(false);
        return new AdditionalFile(GlobalConfigFile, fileContent.Replace("is_global = true", string.Empty));
    }
}

internal sealed class CustomAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions { get; } = new CustomAnalyzerConfigOptions(globalOptions);

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

    private class CustomAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
    {
        public static CustomAnalyzerConfigOptions Empty { get; } = new([]);

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => options.TryGetValue(key, out value);

        public override IEnumerable<string> Keys => options.Keys;
    }

    public static async Task<CustomAnalyzerConfigOptionsProvider> LoadGlobalConfigAsync()
    {
        string editorConfigPath = Path.Combine(AppContext.BaseDirectory, AdditionalFile.GlobalConfigFile);

        var settings = new Dictionary<string, string>();
        foreach (string line in await File.ReadAllLinesAsync(editorConfigPath).ConfigureAwait(false))
        {
            if (line.StartsWith("dotnet_diagnostic") && line.Split('=') is [string key, string value])
                settings[key.Trim()] = value.Trim();
        }
        return new(settings);
    }
}
