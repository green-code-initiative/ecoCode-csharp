namespace EcoCode.Tests;

[TestClass]
public class SpecifyStructLayoutUnitTests
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        SpecifyStructLayoutAnalyzer,
        SpecifyStructLayoutCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task EmptyStructWithNoLayoutAsync() => await VerifyAsync(
        "public record struct TestStruct;").ConfigureAwait(false);

    [TestMethod]
    public async Task ValuePropWithNoLayoutAsync() => await VerifyAsync(
        "public record struct TestStruct(int A);").ConfigureAwait(false);

    [TestMethod]
    public async Task ReferencePropWithNoLayoutAsync() => await VerifyAsync(
        "public record struct TestStruct(string A);").ConfigureAwait(false);

    [TestMethod]
    public async Task ValuePropsWithNoLayoutAsync() => await VerifyAsync(
        "public record struct [|TestStruct|](int A, double B);",
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public record struct TestStruct(int A, double B);
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValuePropsWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public record struct TestStruct(int A, double B);
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueAndReferencePropsWithNoLayoutAsync() => await VerifyAsync(
        "public record struct TestStruct(int A, string B);").ConfigureAwait(false);

    [TestMethod]
    public async Task AdditionalValuePropsWithNoLayout1Async() => await VerifyAsync(
        "public record struct [|TestStruct|](int A, double B, int C);",
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public record struct TestStruct(int A, double B, int C);
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AdditionalValueFieldsWithNoLayout2Async() => await VerifyAsync(
        "public record struct [|TestStruct|](bool A, int B, char C, short D, ulong E, System.DateTime F);",
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public record struct [|TestStruct|](bool A, int B, char C, short D, ulong E, System.DateTime F);
        """).ConfigureAwait(false);
}
