using EcoCode.Core.Analyzers;

namespace EcoCode.Tests;

[TestClass]
public sealed class UseCastInsteadOfSelectToCastTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseCastInsteadOfSelectToCast, UseCastInsteadOfSelectToCastFixer>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task SelectMethodUsedForStringArrayAsync() => await VerifyAsync("""
    using System.Linq;
    using System.Collections.Generic;
    public class Program
    {
        public static void Main()
        {
            var strings = GetStrings();
            var stringsAsObjects = [|strings.Select(s => (object)s)|].ToList();
        }

        private static IEnumerable<string> GetStrings()
        {
            return new List<string> { "Hello", "World" };
        }
    }
    """, """
    using System.Linq;
    using System.Collections.Generic;
    public class Program
    {
        public static void Main()
        {
            var strings = GetStrings();
            var stringsAsObjects = strings.Cast<object>().ToList();
        }

        private static IEnumerable<string> GetStrings()
        {
            return new List<string> { "Hello", "World" };
        }
    }
    """).ConfigureAwait(false);

    [TestMethod]
    public async Task CastMethodUsedForNumberArrayAsync() => await VerifyAsync("""
        using System.Linq;
        public class Test
        {
            public void Run()
            {
                var numbers = new int[] { 1, 2, 3, 4, 5 };
                var castedNumbers = numbers.Cast<double>().ToList();

                var numbers2 = new int[] { 6, 7, 8, 9, 10 };
                var correctlyCastedNumbers = numbers2.Cast<double>().ToList();
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CastMethodUsedForStringArrayAsync() => await VerifyAsync("""
    using System.Linq;
    using System.Collections.Generic;
    public class Test
    {
        public void Run()
        {
            IEnumerable<string> strings = GetStrings();
            var stringsAsObjects = strings.Cast<object>().ToList();
        }

        private IEnumerable<string> GetStrings()
        {
            return new List<string> { "Hello", "World" };
        }
    }
    """).ConfigureAwait(false);


    [TestMethod]
    public async Task CastMethodUsedForEmptyArrayAsync() => await VerifyAsync("""
        using System.Linq;
        public class Test
        {
            public void Run()
            {
                var numbers = new int[] { };
                var castedNumbers = numbers.Cast<double>().ToList();
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CastMethodUsedForArrayWithNullValuesAsync() => await VerifyAsync("""
    using System.Linq;
    public class Test
    {
        public void Run()
        {
            var strings = new string[] { "Hello", null, "World" };
            var castedStrings = strings.Cast<object>().ToList();
        }
    }
    """).ConfigureAwait(false);


    [TestMethod]
    public async Task CastMethodUsedForBoolArrayAsync() => await VerifyAsync("""
        using System.Linq;
        public class Test
        {
            public void Run()
            {
                var booleans = new bool[] { true, false, true };
                var castedBooleans = booleans.Cast<object>().ToList();
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task CastMethodUsedForNestedArrayAsync() => await VerifyAsync("""
        using System.Linq;
        public class Test
        {
            public void Run()
            {
                var nestedArrays = new int[][] { new int[] {1, 2}, new int[] {3, 4} };
                var castedNestedArrays = nestedArrays.Cast<object>().ToList();
            }
        }
        """).ConfigureAwait(false);
}
