using System.Collections.Generic;

namespace EcoCode.Tests;

[TestClass]
public sealed class DontConcatenateStringsInLoopsTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<DontConcatenateStringsInLoops>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    #region Regular loops

    private const string WarningCode = """

        {
            string si = i.ToString();
            [|s += si|];
            [|s = s + si|];
            [|s = si + s|];
            s = si + si;
        }
        """;

    [TestMethod]
    public async Task DontConcatenateStringsInLoopsParameterWithAllLoopsAsync() => await VerifyAsync($$"""
        using System.Linq;
        class Test
        {
            void Run1(string s)
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }

            void Run2(string s)
            {
                foreach (int i in Enumerable.Range(0, 10)){{WarningCode}}
            }

            void Run3(string s)
            {
                int i = 0;
                while (i++ < 10){{WarningCode}}
            }

            void Run4(string s)
            {
                int i = 0;
                do{{WarningCode}} while (++i < 10);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoopsFieldAsync() => await VerifyAsync($$"""
        class Test
        {
            string s = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoopsPropertyAsync() => await VerifyAsync($$"""
        class Test
        {
            string s { get; set; } = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoopsStaticFieldAsync() => await VerifyAsync($$"""
        class Test
        {
            static string s = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoopsStaticPropertyAsync() => await VerifyAsync($$"""
        class Test
        {
            static string s { get; set; } = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInLoopsLocalVariableAsync() => await VerifyAsync($$"""
        class Test
        {
            void Run()
            {
                for (int i = 0; i < 10; i++)
                {
                    string s = string.Empty;
                    s += i;
                    s = s + i.ToString();
                    s = i.ToString();
                    s = i.ToString() + s;
                }
            }
        }
        """).ConfigureAwait(false);

    #endregion

    #region ForEach methods

    [TestMethod]
    public async Task DontConcatenateStringsInForEachAsync() => await VerifyAsync("""
        using System.Collections.Generic;
        public class Test
        {
            private string s1 = string.Empty;
            private static string s2 = string.Empty;

            public void Run(string s0)
            {
                string s3 = string.Empty;
                var list = new List<int>();
                list.ForEach(i =>
                {
                    s0 = i.ToString();
                    [|s0 += i|];

                    s1 = i.ToString();
                    [|s1 += i|];

                    s2 = i.ToString();
                    [|s2 += i|];

                    s3 = i.ToString();
                    [|s3 += i|];

                    string s4 = string.Empty;
                    s4 = i.ToString();
                    s4 += i;
                });
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEach1Async() => await VerifyAsync("""
        using System.Collections.Generic;
        class Test
        {
            void Run(string s)
            {
                var list = new List<int>();
                list.ForEach(_ =>
                {
                    [|s = s + 'A'|];
                });
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEach2Async() => await VerifyAsync("""
        using System.Collections.Generic;
        class Test
        {
            void Run(string s)
            {
                var list = new List<int>();
                list.ForEach(_ => [|s += 'A'|]);
            }
        }
        """).ConfigureAwait(false);

    public static void Run11()
    {
        string s = string.Empty;
        var list = new List<int>();
        list.ForEach(i =>
        {
            s += i;
            s = s + 'A';
        });
    }

    [TestMethod]
    public async Task DontConcatenateStringsInForEachXXAsync() => await VerifyAsync("""
        using System.Collections.Generic;
        public class Test
        {
            public void Run(string s)
            {
                var list = new List<int>();
                list.ForEach(i =>
                {
                    s = i.ToString();
                    [|s += i|];
                });
            }
        }
        """).ConfigureAwait(false);

    // Cas 1 : lambda
    // 1.1 : body with no parameter
    // 1.2 : body with a single parameter without parenthesis
    // 1.3 : body with parenthesis
    // 2.1 : expression body with no parameter
    // 2.2 : expression body with a single parameter without parenthesis
    // 2.3 : expression body with parenthesis

    // Cas 2 : direct function call

    #endregion
}
