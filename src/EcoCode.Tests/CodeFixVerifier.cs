namespace EcoCode.Tests;

public delegate Task VerifyDlg(string source, DiagnosticResult? expected = null, string? fixedSource = null);

internal static class CodeFixVerifier
{
    public static async Task VerifyAsync<TAnalyzer, TCodeFix>(string source, DiagnosticResult? expected = null, string? fixedSource = null)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier> { TestCode = source };
        if (fixedSource is not null)
            test.FixedCode = fixedSource;
        if (expected is not null)
            test.ExpectedDiagnostics.Add(expected.GetValueOrDefault());
        await test.RunAsync().ConfigureAwait(false);
    }
}
