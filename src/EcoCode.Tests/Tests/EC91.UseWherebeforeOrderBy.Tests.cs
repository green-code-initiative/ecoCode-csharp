namespace EcoCode.Tests;

[TestClass]
public sealed class UseWhereBeforeOrderByTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseWhereBeforeOrderBy, UseWhereBeforeOrderByFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    #region Method syntax

    [TestMethod]
    public async Task DontWarnOnWhereOnlyMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .Where(x => x > 10)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnOrderByOnlyMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .OrderBy(x => x)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnOrderByDescendingOnlyMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .OrderByDescending(x => x)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnRightOrderMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongOrderMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.OrderBy(x => x).[|Where|](x => x > 10).Select(x => x);
                var query2 = items
                    .OrderBy(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.Where(x => x > 10).OrderBy(x => x).Select(x => x);
                var query2 = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .Select(x => x);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongOrderDescendingMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.OrderByDescending(x => x).[|Where|](x => x > 10).Select(x => x);
                var query2 = items
                    .OrderByDescending(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.Where(x => x > 10).OrderByDescending(x => x).Select(x => x);
                var query2 = items
                    .Where(x => x > 10)
                    .OrderByDescending(x => x)
                    .Select(x => x);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongMultipleOrderMethodAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.OrderBy(x => x).ThenByDescending(x => x).ThenBy(x => x).[|Where|](x => x > 10).Select(x => x);
                var query2 = items
                    .OrderBy(x => x)
                    .ThenByDescending(x => x)
                    .ThenBy(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.Where(x => x > 10).OrderBy(x => x).ThenByDescending(x => x).ThenBy(x => x).Select(x => x);
                var query2 = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .ThenByDescending(x => x)
                    .ThenBy(x => x)
                    .Select(x => x);
            }
        }
        """).ConfigureAwait(false);

    #endregion

    #region Query syntax

    [TestMethod]
    public async Task DontWarnOnWhereOnlyQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            select item;
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnOrderByOnlyQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item
                            select item;
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnOrderByDescendingOnlyQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item descending
                            select item;
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnRightOrderQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            orderby item
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongOrderQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item
                            [|where|] item > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            orderby item
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongMultipleOrderQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query = from item in items
                            orderby item.A
                            orderby item.B
                            [|where|] item.A > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query = from item in items
                            where item.A > 10
                            orderby item.A
                            orderby item.B
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnDisjointedOrderQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item
                            select item
                            into item
                            where item > 0
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongOrderDescendingQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item descending
                            [|where|] item > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            orderby item descending
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontWarnOnDisjointedOrderDescendingQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item descending
                            select item
                            into item
                            where item > 0
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task WarnOnWrongMultipleOrderDescendingQueryAsync() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query = from item in items
                            orderby item.A descending
                            orderby item.B descending
                            [|where|] item.A > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query = from item in items
                            where item.A > 10
                            orderby item.A descending
                            orderby item.B descending
                            select item;
            }
        }
        """).ConfigureAwait(false);

    #endregion
}
