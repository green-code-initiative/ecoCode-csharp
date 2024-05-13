namespace EcoCode.Tests;

[TestClass]
public sealed class AvoidAsyncVoidMethodsTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<AvoidAsyncVoidMethods, AvoidAsyncVoidMethodsFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task AvoidAsyncVoidMethodAsync() => await VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Program
        {
            public static async void [|Main|]()
            {
                await Task.Delay(1000).ConfigureAwait(false);
                Console.WriteLine();
            }
        }
        """, """
        using System;
        using System.Threading.Tasks;
        public static class Program
        {
            public static async Task Main()
            {
                await Task.Delay(1000).ConfigureAwait(false);
                Console.WriteLine();
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AvoidAsyncVoidMethodWithMissingUsingAsync() => await VerifyAsync("""
        using System;
        using System.Net.Http;
        public static class Program
        {
            public static async void [|Main|]()
            {
                using var httpClient = new HttpClient();
                _ = await httpClient.GetAsync(new Uri("URL")).ConfigureAwait(false);
            }
        }
        """, """
        using System;
        using System.Net.Http;
        public static class Program
        {
            public static async {|CS0246:Task|} {|CS0161:Main|}()
            {
                using var httpClient = new HttpClient();
                _ = await httpClient.GetAsync(new Uri("URL")).ConfigureAwait(false);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AsyncTaskMethodIsOkAsync() => await VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Program
        {
            public static async Task Main()
            {
                await Task.Delay(1000).ConfigureAwait(false);
                Console.WriteLine();
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AsyncGenericTaskMethodIsOkAsync() => await VerifyAsync("""
        using System.Threading.Tasks;
        public static class Program
        {
            public static async Task<int> Main()
            {
                await Task.Delay(1000).ConfigureAwait(false);
                return 1;
            }
        }
        """).ConfigureAwait(false);
}
