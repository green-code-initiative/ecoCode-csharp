using Microsoft.CodeAnalysis.CSharp;

namespace EcoCode.Tests;

[TestClass]
public sealed class UseCollectionAnalyzerUnitTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseCollectionIndexer, UseCollectionIndexerFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    #region First

    [TestMethod]
    public Task RefactorFirstAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return [|list.First|]();
            }
        }
        """, """
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return list[0];
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstWithNoIndexerAsync() => VerifyAsync("""
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var enumerable = Enumerable.Range(0, 10);
                return enumerable.First();
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstWithPredicateAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return list.First(x => x != 0);
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstIfNotLinqAsync() => VerifyAsync("""
        using System.Collections.Generic;
        public class MyCollection<T> : List<T>
        {
            public T First() => this[0];
        }
        public static class Test
        {
            public static int GetFirstValue()
            {
                var myCollection = new MyCollection<int> { 1, 2, 3 };
                return myCollection.First();
            }
        }
        """);

    #endregion

    #region Last

    [TestMethod]
    public Task RefactorLastWithIndexerAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return [|list.Last|]();
            }
        }
        """, """
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return list[^1];
            }
        }
        """);

    [TestMethod]
    public Task RefactorLastWithCountOrLengthAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return [|list.Last|]();
            }
        }
        """, """
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return list[list.Count - 1];
            }
        }
        """, languageVersion: LanguageVersion.CSharp7);

    [TestMethod]
    public Task DontRefactorLastWithNoIndexerAsync() => VerifyAsync("""
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var enumerable = Enumerable.Range(0, 10);
                return enumerable.Last();
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorLastWithPredicateAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return list.Last(x => x != 0);
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorLastIfNotLinqAsync() => VerifyAsync("""
        using System.Collections.Generic;
        public class MyCollection<T> : List<T>
        {
            public T Last() => this[^1];
        }
        public static class Test
        {
            public static int GetFirstValue()
            {
                var myCollection = new MyCollection<int> { 1, 2, 3 };
                return myCollection.Last();
            }
        }
        """);

    #endregion

    #region ElementAt

    [TestMethod]
    public Task UseIndexerInsteadOfElementAtAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return [|list.ElementAt|](2);
            }
        }
        """, """
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var list = new List<int> { 1, 2, 3 };
                return list[2];
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorElementAtWithNoIndexerAsync() => VerifyAsync("""
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var enumerable = Enumerable.Range(0, 10);
                return enumerable.ElementAt(1);
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorElementAtIfNotLinqAsync() => VerifyAsync("""
        using System.Collections.Generic;
        public class MyCollection<T> : List<T>
        {
            public T ElementAt(int index) => this[index];
        }
        public static class Test
        {
            public static int GetFirstValue()
            {
                var myCollection = new MyCollection<int> { 1, 2, 3 };
                return myCollection.ElementAt(2);
            }
        }
        """);

    #endregion
}
