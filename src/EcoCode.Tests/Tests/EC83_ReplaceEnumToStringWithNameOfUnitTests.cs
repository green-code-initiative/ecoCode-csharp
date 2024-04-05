namespace EcoCode.Tests;

[TestClass]
public class ReplaceEnumToStringWithNameOfUnitTests
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        ReplaceEnumToStringWithNameOfAnalyzer,
        ReplaceEnumToStringWithNameOfCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task EnumToStringShouldBeNameOfAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine([|MyEnum.A.ToString()|]);
                Console.WriteLine([|MyEnum.B.ToString("")|]);
                Console.WriteLine([|MyEnum.C.ToString(string.Empty)|]);
                Console.WriteLine([|MyEnum.D.ToString(format: null)|]);
            }
        }
        """,
        fixedSource: """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine(nameof(MyEnum.A));
                Console.WriteLine(nameof(MyEnum.B));
                Console.WriteLine(nameof(MyEnum.C));
                Console.WriteLine(nameof(MyEnum.D));
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task EnumInterpolationShouldBeNameOfAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine($"[|{MyEnum.A}|]");
                Console.WriteLine($"[|{MyEnum.B:G}|]");
                Console.WriteLine($"[|{MyEnum.C:F}|]");
            }
        }
        """,
        fixedSource: """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine($"{nameof(MyEnum.A)}");
                Console.WriteLine($"{nameof(MyEnum.B)}");
                Console.WriteLine($"{nameof(MyEnum.C)}");
            }
        }
        """).ConfigureAwait(false);
}
