namespace EcoCode.Tests;

[TestClass]
public sealed class MakeTypeSealed
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        MakeTypeSealedAnalyzer,
        MakeTypeSealedCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task MakeClassSealedAsync() => await VerifyAsync("public class [|Test|];", fixedSource: "public sealed class Test;").ConfigureAwait(false);

    [TestMethod]
    public async Task MakeRecordSealedAsync() => await VerifyAsync("public record [|Test|];", fixedSource: "public sealed record Test;").ConfigureAwait(false);
}
