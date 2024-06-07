namespace EcoCode.Tests;

[TestClass]
public sealed class DontConcatenateStringsInLoopsTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<DontConcatenateStringsInLoops>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    private const string WarningCode = """

        {
            string si = i.ToString();
            [|s += si|];
            [|s = s + si|];
            [|s = si + s|];
            s = si + si;
        }
        """;

    #region Regular loops

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
                string s = string.Empty;
                for (int i = 0; i < 10; i++){{WarningCode}}

                for (int i = 0; i < 10; i++)
                {
                    string si = i.ToString();
                    string s2 = string.Empty;
                    s2 += si;
                    s2 = s2 + si;
                    s2 = si + s2;
                    s2 = si + si;
                }
            }
        }
        """).ConfigureAwait(false);

    #endregion

    #region ForEach methods

    [TestMethod]
    public async Task DontConcatenateStringsInForEachParameterAsync() => await VerifyAsync($$"""
        using System.Collections.Generic;
        public class Test
        {
            public void Run(List<int> list, string s)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEachFieldAsync() => await VerifyAsync($$"""
        using System.Collections.Generic;
        public class Test
        {
            private string s = string.Empty;
            public void Run(List<int> list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEachPropertyAsync() => await VerifyAsync($$"""
        using System.Collections.Generic;
        public class Test
        {
            private string s { get; set; } = string.Empty;
            public void Run(List<int> list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEachStaticFieldAsync() => await VerifyAsync($$"""
        using System.Collections.Generic;
        public class Test
        {
            private static string s = string.Empty;
            public void Run(List<int> list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEachStaticPropertyAsync() => await VerifyAsync($$"""
        using System.Collections.Generic;
        public class Test
        {
            private static string s { get; set; } = string.Empty;
            public void Run(List<int> list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontConcatenateStringsInForEachLocalVariableAsync() => await VerifyAsync($$"""
        using System.Collections.Generic;
        public class Test
        {
            public void Run(List<int> list)
            {
                string s = string.Empty;
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());

                list.ForEach(i =>
                {
                    string si = i.ToString();
                    string s2 = string.Empty;
                    s2 += si;
                    s2 = s2 + si;
                    s2 = si + s2;
                    s2 = si + si;
                });
            }
        }
        """).ConfigureAwait(false);

    #endregion
}
