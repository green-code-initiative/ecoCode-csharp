namespace EcoCode.Tests;

public delegate Task VerifyDlg(string source, string? fixedSource = null);

internal static class CodeFixVerifier
{
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
