namespace EcoCode.Tests;

[TestClass]
public sealed class DontCallFunctionsInLoopConditionsTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<DontCallFunctionsInLoopConditions>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task TestAsync() => await VerifyAsync("""
        using System;
        using System.IO;
        public static class Test
        {
            public static void Run(string path)
            {
                while (!path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase))
                    path = Path.GetDirectoryName(path)!;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task ForLoopFunctionCallShouldNotBeCalledAsync() => await VerifyAsync("""
        public class Test
        {
            private const int C = 10;
            private static int V1 => 10;
            private static int V2() => 10;
            private static int V3(int i) => i;

            public void Run(int p)
            {
                int j = 0, k = 10;

                for (int i = 0; i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]; i++)
                    j += i;

                for (int i = 0; i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]; i++)
                {
                    j += i;
                }
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WhileLoopFunctionCallShouldNotBeCalledAsync() => await VerifyAsync("""
        public class Test
        {
            private const int C = 10;
            private static int V1 => 10;
            private static int V2() => 10;
            private static int V3(int i) => i;

            public void Run(int p)
            {
                int i = 0, j = 0, k = 10;

                while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|])
                    j += i++;

                while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|])
                {
                    j += i++;
                }
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DoWhileLoopFunctionCallShouldNotBeCalledAsync() => await VerifyAsync("""
        public class Test
        {
            private const int C = 10;
            private static int V1 => 10;
            private static int V2() => 10;
            private static int V3(int i) => i;

            public void Run(int p)
            {
                int i = 0, j = 0, k = 10;
                
                do j += i++;
                while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]);

                do
                {
                    j += i++;
                } while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]);
            }
        }
        """).ConfigureAwait(false);
}
