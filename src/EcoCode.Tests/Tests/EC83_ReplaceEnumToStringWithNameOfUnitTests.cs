namespace EcoCode.Tests;

[TestClass]
public sealed class ReplaceEnumToStringWithNameOfUnitTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<ReplaceEnumToStringWithNameOf, ReplaceEnumToStringWithNameOfFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task EnumToStringShouldBeNameOfAsync1() => await VerifyAsync("""
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
        """, """
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
    public async Task EnumToStringShouldBeNameOfAsync2() => await VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C }
            public static void Main()
            {
                Console.WriteLine([|MyEnum.A.ToString("G")|]);
                Console.WriteLine([|MyEnum.B.ToString("F")|]);
                Console.WriteLine(MyEnum.C.ToString("N"));
            }
        }
        """, """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C }
            public static void Main()
            {
                Console.WriteLine(nameof(MyEnum.A));
                Console.WriteLine(nameof(MyEnum.B));
                Console.WriteLine(MyEnum.C.ToString("N"));
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
                Console.WriteLine($"{MyEnum.D:N}");
            }
        }
        """, """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine($"{nameof(MyEnum.A)}");
                Console.WriteLine($"{nameof(MyEnum.B)}");
                Console.WriteLine($"{nameof(MyEnum.C)}");
                Console.WriteLine($"{MyEnum.D:N}");
            }
        }
        """).ConfigureAwait(false);
}
