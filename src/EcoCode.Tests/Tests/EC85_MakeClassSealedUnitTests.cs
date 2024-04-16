namespace EcoCode.Tests;

[TestClass]
public sealed class MakeClassSealed
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        MakeClassSealedAnalyzer,
        MakeClassSealedCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task MakeClassSealedAsync() => await VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public class [|Program|]
        {
            public void Main() => Console.WriteLine();
        }
        """).ConfigureAwait(false);
}
