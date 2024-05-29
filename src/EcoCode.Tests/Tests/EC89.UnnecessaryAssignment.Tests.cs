namespace EcoCode.Tests.Tests;

[TestClass]
public sealed class UnnecessaryAssignmentTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UnnecessaryAssignment>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);
    
    [TestMethod]
    public Task DoNotWarmOnSimpleIfStatement() => VerifyAsync("""
        public class Test
        {
            int Number()
            {
                int x = 1;
                if (x > 1)
                {
                    x = 2;
                }

                return x;
            }
        }
        """);

    [TestMethod]
    public Task WarmOnBasicUnnecessaryAssignment() => VerifyAsync("""
        public class Test
        {
            int Number()
            {
                int x = 1;
                [|if (x > 1)
                {
                    x = 2;
                }
                else
                {
                    x = 3;
                }|]

                return x;
            }
        }
        """);

    [TestMethod]
    public Task WarmOnMultipleIfElseUnnecessaryAssignment() => VerifyAsync("""
        public class Test
        {
            int Number()
            {
                int x = 1;
                [|if (x > 1)
                {
                    x = 2;
                }
                else if (x < 1)
                {
                    x = 3;
                }
                else
                {
                    x = 4;
                }|]

                return x;
            }
        }
        """);
}
