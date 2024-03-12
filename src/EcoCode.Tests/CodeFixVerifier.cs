namespace EcoCode.Tests;

public static class CodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic() => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic();

    public static DiagnosticResult Diagnostic(string diagnosticId) => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic(descriptor);

    public static async Task VerifyAsync(string source, DiagnosticResult? expected = null, string? fixedSource = null)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier> { TestCode = source };
        if (fixedSource is not null)
            test.FixedCode = fixedSource;
        if (expected is not null)
            test.ExpectedDiagnostics.Add(expected.GetValueOrDefault());
        await test.RunAsync().ConfigureAwait(false);
    }
}