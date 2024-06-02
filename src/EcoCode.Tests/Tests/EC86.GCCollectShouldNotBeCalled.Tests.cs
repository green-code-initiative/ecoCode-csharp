namespace EcoCode.Tests;

[TestClass]
public class GCCollectShouldNotBeCalledTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<GCCollectShouldNotBeCalled>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect()|];
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledSystemAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|System.GC.Collect()|];
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledNamedArgumentsAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect(mode: GCCollectionMode.Optimized, generation: 1)|];
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledMultipleCodeAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                string text="";       [|GC.Collect()|]; string text2 = "";
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledCommentedAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                //GC.Collect();
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledGeneration0Async() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                GC.Collect(0);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledGeneration10Async() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect(10)|];
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledGeneratio0CollectionModeAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                GC.Collect(0, GCCollectionMode.Forced);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeCalledGeneration10CollectionModeAsync() => await VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect(10, GCCollectionMode.Forced)|];
            }
        }
        """).ConfigureAwait(false);
}
