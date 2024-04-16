namespace EcoCode.Tests;

[TestClass]
public sealed class SpecifyStructLayoutUnitTests
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
    // For some reason this test doesn't pass with the 'record' syntax on GitHub, but it passes locally..
    public async Task ValuePropsWithNoLayoutAsync() => await VerifyAsync("""
        public struct [|TestStruct|]
        {
            public int A { get; set; }
            public double B { get; set; }
        };
        """,
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A { get; set; }
            public double B { get; set; }
        };
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
    // For some reason this test doesn't pass with the 'record' syntax on GitHub, but it passes locally..
    public async Task AdditionalValuePropsWithNoLayout1Async() => await VerifyAsync("""
        public struct [|TestStruct|]
        {
            public int A { get; set; }
            public double B { get; set; }
            public int C { get; set; }
        };
        """,
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A { get; set; }
            public double B { get; set; }
            public int C { get; set; }
        };
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AdditionalValueFieldsWithNoLayout2Async() => await VerifyAsync("""
        using System;

        public record struct [|TestStruct|](bool A, int B, char C, short D, ulong E, DateTime F);
        """,
        fixedSource: """
        using System;
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public record struct [|TestStruct|](bool A, int B, char C, short D, ulong E, DateTime F);
        """).ConfigureAwait(false);
}
