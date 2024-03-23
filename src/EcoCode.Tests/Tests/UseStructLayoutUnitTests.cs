namespace EcoCode.Tests;

[TestClass]
public class UseStructLayoutUnitTests
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        UseStructLayout,
        UseStructLayoutCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task EmptyStructWithNoLayoutAsync() => await VerifyAsync("""
        public struct TestStruct
        {
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task EmptyStructWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueFieldWithNoLayoutAsync() => await VerifyAsync("""
        public struct TestStruct
        {
            public int A;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueFieldWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValuePropertyWithNoLayoutAsync() => await VerifyAsync("""
        public struct TestStruct
        {
            public int A { get; set; }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValuePropertyWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A { get; set; }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ReferenceFieldWithNoLayoutAsync() => await VerifyAsync("""
        public struct TestStruct
        {
            public string A;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ReferenceFieldWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public string A;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueFieldsWithNoLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;

        public struct [|TestStruct|]
        {
            public int A;
            public double B;
        }
        """,
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A;
            public double B;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueFieldsWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A;
            public double B;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueAndReferenceFieldsWithNoLayoutAsync() => await VerifyAsync("""
        public struct TestStruct
        {
            public int A;
            public string B;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ValueAndReferenceFieldsWithLayoutAsync() => await VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A;
            public string B;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AdditionalValueFieldsWithNoLayout1Async() => await VerifyAsync("""
        using System.Runtime.InteropServices;

        public struct [|TestStruct|]
        {
            public int A;
            public double B;
            public int C;
        }
        """,
        fixedSource: """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A;
            public double B;
            public int C;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AdditionalValueFieldsWithNoLayout2Async() => await VerifyAsync("""
        using System;
        using System.Runtime.InteropServices;

        public struct [|TestStruct|]
        {
            public bool A;
            public int B;
            public char C;
            public short D;
            public ulong E;
            public DateTime F;
        }
        """,
        fixedSource: """
        using System;
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public bool A;
            public int B;
            public char C;
            public short D;
            public ulong E;
            public DateTime F;
        }
        """).ConfigureAwait(false);
}
