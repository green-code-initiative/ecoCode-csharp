namespace EcoCode.Tests;

[TestClass]
public sealed class DisposeAsynchronouslyTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<DisposeAsynchronously, DisposeAsynchronouslyFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public Task DontWarnOnMissingUsingsAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static async Task Run()
            {
                var d1 = new DisposableClass();
                Console.WriteLine(d1);

                var d2 = new AsyncDisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnNonAsyncableUsingsAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }

            public static async Task Run()
            {
                using (var d1 = new DisposableClass())
                    Console.WriteLine(d1);

                using var d2 = new DisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnAsyncableUsingsInAsyncMethodAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static async Task Run()
            {
                [|using|] (var d1 = new AsyncDisposableClass())
                    Console.WriteLine(d1);

                [|using|] var d2 = new AsyncDisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnAsyncableUsingsInNonAsyncMethodAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static void Run()
            {
                using var d1 = new AsyncDisposableClass();
                Console.WriteLine(d1);

                using var d2 = new AsyncDisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);
}
