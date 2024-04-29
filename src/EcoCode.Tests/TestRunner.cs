namespace EcoCode.Tests;

public delegate Task AnalyzerDlg(string source);
public delegate Task CodeFixerDlg(string source, string? fixedSource = null);

internal static class TestRunner
{
    public static async Task VerifyAsync<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, MSTestVerifier> { TestCode = source };
        await test.RunAsync().ConfigureAwait(false);
    }

    public static async Task VerifyAsync<TAnalyzer, TCodeFix>(string source, string? fixedSource = null)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier> { TestCode = source };
        if (fixedSource is not null)
            test.FixedCode = fixedSource;
        await test.RunAsync().ConfigureAwait(false);
    }
}
