using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis;

namespace EcoCode.Tests;

[TestClass]
public sealed class UseStringEmptyLengthTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UseStringEmptyLength>;
    
    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public Task DontUseStringEmptyLengthWithEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|test == ""|]) { }
            }
        }
        """);

    [TestMethod]
    public Task DontUseStringEmptyLengthReverseWithEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|"" == test|]) { }
            }
        }
        """);

    [TestMethod]
    public Task DontUseStringEmptyLengthWithNotEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|"" != test|]) { }
            }
        }
        """);

    [TestMethod]
    public Task UseStringEmptyLengthAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if (test.Length == 0) { }
            }
        }
        """);
}
