namespace EcoCode.Tests;

[TestClass]
public sealed class ReturnTaskDirectlyTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<ReturnTaskDirectly, ReturnTaskDirectlyFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public Task DontWarnOnMissingUsingsAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run1()
            {
                await Task.Delay(0);
            }

            public static [|async|] Task Run2()
            {
                // Test
                await Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnMissingUsings3Async() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run1() => await Task.Delay(0);
            public static [|async|] Task Run2() =>
                // Test
                await Task.Delay(0);
        }
        """);

    [TestMethod]
    public Task DontWarnOnMissingUsings2Async() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run1() => Task.Delay(0);
            public static Task Run2()
            {
                return Task.Delay(0);
            }
            public static async Task Run3()
            {
                Console.WriteLine();
                await Task.Delay(0);
            }
        }
        """);
}
