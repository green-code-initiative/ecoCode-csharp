using Microsoft;

namespace EcoCode.Tests;

[TestClass]
public sealed class UseWhereBeforeOrderByTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseWhereBeforeOrderBy, UseWhereBeforeOrderByFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task NoDiagnosticForCorrectOrder() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items = new List<int>();

            public void TestMethod()
            {
                var query = from item in items
                            where item > 10
                            orderby item
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForIncorrectOrderWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items = new List<int>();

            public void TestMethod()
            {
                var query = items
                    .OrderBy(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForIncorrectOrderByDescendingWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items = new List<int>();

            public void TestMethod()
            {
                var query = items
                    .OrderByDescending(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForCorrectOrderWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items = new List<int>();

            public void TestMethod()
            {
                var query = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForCorrectOrderOrderbyOnlyWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items = new List<int>();

            public void TestMethod()
            {
                var query = items
                    .OrderBy(x => x)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForCorrectOrderWhereOnlyWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items = new List<int>();

            public void TestMethod()
            {
                var query = items
                    .Where(x => x > 10)
                    .Select(x => x);
            }            
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForIncorrectOrder() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items =new List<int>();
                
            public void TestMethod()
            {
                var query = from item in items
                            [|orderby item|]
                            where item > 10
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DiagnosticForIncorrectOrderDescending() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public class TestClass
        {
            var items =new List<int>();
                
            public void TestMethod()
            {
                var query = from item in items
                            [|orderby item descending|]
                            where item > 10
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CodeFixForIncorrectOrder() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items =new List<int>();
            
            public void TestMethod()
            {
                var query = from item in items
                            [|orderby item descending|]
                            where item > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items =new List<int>();
            
            public void TestMethod()
            {
                var query = from item in items
                            where item > 10
                            orderby item descending
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CodeFixForIncorrectOrderDescending() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items =new List<int>();
            
            public void TestMethod()
            {
                var query = from item in items
                            [|orderby item descending|]
                            where item > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items =new List<int>();
            
            public void TestMethod()
            {
                var query = from item in items
                            where item > 10
                            orderby item descending
                            select item;
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CodeFixForIncorrectOrderWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items = new List<int>();
            
            public void TestMethod()
            {
                var query = items
                    .OrderBy(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }            
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items = new List<int>();
            
            public void TestMethod()
            {
                var query = items

                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .Select(x => x);
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CodeFixForIncorrectOrderDesendingWithObject() => await VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items = new List<int>();
            
            public void TestMethod()
            {
                var query = items
                    .OrderByDescending(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }            
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
            
        public class TestClass
        {
            var items = new List<int>();
            
            public void TestMethod()
            {
                var query = items
            
                    .Where(x => x > 10)
                    .OrderByDescending(x => x)
                    .Select(x => x);
            }
        }
        """).ConfigureAwait(false);
}
