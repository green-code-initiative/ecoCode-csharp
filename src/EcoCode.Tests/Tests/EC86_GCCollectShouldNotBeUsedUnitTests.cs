namespace EcoCode.Tests;

[TestClass]
public class GCCollectShouldNotBeUsedUnitTests
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        GCCollectShouldNotBeUsedAnalyzer,
        GCCollectShouldNotBeUsedFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task GCCollectShouldNotBeUsedAsync() => await VerifyAsync("""
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
    public async Task GCCollectShouldNotBeUsedMultipleCodeAsync() => await VerifyAsync("""
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
    public async Task GCCollectShouldNotBeUsedCommentedAsync() => await VerifyAsync("""
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
    public async Task GCCollectShouldNotBeUsedGeneration0Async() => await VerifyAsync("""
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
    public async Task GCCollectShouldNotBeUsedGeneration10Async() => await VerifyAsync("""
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
    public async Task GCCollectShouldNotBeUsedGeneratio0CollectionModeAsync() => await VerifyAsync("""
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
    public async Task GCCollectShouldNotBeUsedGeneration10CollectionModeAsync() => await VerifyAsync("""
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
