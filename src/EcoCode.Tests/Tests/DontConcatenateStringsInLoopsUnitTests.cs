using Verifier = EcoCode.Tests.CodeFixVerifier<
    EcoCode.Analyzers.DontConcatenateStringsInLoops,
    EcoCode.CodeFixes.DontConcatenateStringsInLoopsCodeFixProvider>;

namespace EcoCode.Tests;

[TestClass]
public class DontConcatenateStringsInLoopsUnitTests
{
    [TestMethod]
    public async Task EmptyCodeAsync() => await Verifier.VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoops1Async() => await Verifier.VerifyAsync("""
        public class Test
        {
            private string s1 = string.Empty;
            private static string s2 = string.Empty;

            public void Run(string s0)
            {
                for (int i = 0; i < 10; i++)
                    [|s0 += i;|]
                for (int i = 0; i < 10; i++)
                    s0 = i.ToString(provider: null);
                Console.WriteLine(s0);

                for (int i = 0; i < 10; i++)
                    [|s1 += i;|]
                for (int i = 0; i < 10; i++)
                    s1 = i.ToString(provider: null);
                Console.WriteLine(s1);

                for (int i = 0; i < 10; i++)
                    [|s2 += i;|]
                for (int i = 0; i < 10; i++)
                    s2 = i.ToString(provider: null);
                Console.WriteLine(s2);

                string s3 = string.Empty;
                for (int i = 0; i < 10; i++)
                    [|s3 += i;|]
                for (int i = 0; i < 10; i++)
                    s3 = i.ToString(provider: null);
                Console.WriteLine(s3);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoops2Async() => await Verifier.VerifyAsync("""
        public class Test
        {
            private string s1 = string.Empty;
            private static string s2 = string.Empty;

            public void Run(string s0)
            {
                string s3 = string.Empty;
                for (int i = 0; i < 10; i++)
                {
                    s0 = i.ToString(provider: null);
                    [|s0 += i;|]
                    Console.WriteLine(s0);

                    s1 = i.ToString(provider: null);
                    [|s1 += i;|]
                    Console.WriteLine(s1);

                    s2 = i.ToString(provider: null);
                    [|s2 += i;|]
                    Console.WriteLine(s2);

                    s3 = i.ToString(provider: null);
                    [|s3 += i;|]
                    Console.WriteLine(s3);

                    string s4;
                    s4 = i.ToString(provider: null);
                    s4 += i;
                    Console.WriteLine(s4);
                }
            }
        }
        """).ConfigureAwait(false);
}