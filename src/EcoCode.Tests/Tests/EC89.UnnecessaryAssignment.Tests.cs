namespace EcoCode.Tests;

[TestClass]
public sealed class UnnecessaryAssignmentTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UnnecessaryAssignment>;

    [TestMethod]
    public async Task DoNotWarnWhenEmptyCode() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public Task DoNotWarnnOnSimpleIfStatement() => VerifyAsync("""
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
    public Task DonotWarnWhenAssignInIfElseStatementVariableHavePolymorphic() => VerifyAsync("""
            class A {}
            class B {}
            class C
            {
                void M()
                {
                    var fun = (bool flag) =>
                    {
                        object x;
                        if (flag)
                        {
                            x = new A();
                        }
                        else
                        {
                            x = new B();
                        }

                        return x;
                    };
                }
            }
            """);
    
    [TestMethod]
    public Task WarnOnIfElseStatementWithUnnecessaryAssignment() => VerifyAsync("""
            class C
            {
                int M()
                {
                    bool f = false;
                    int x = 1; // x
                    [|if (f)
                    {
                        x = 2;
                    }
                    else if (f)
                    {
                        x = 3;
                    }|]

                    return x;
                }
            }
            """);

    [TestMethod]
    public Task WarnOnMultipleIfElseUnnecessaryAssignment() => VerifyAsync("""
            class C
            {
                int M()
                {
                    bool f = false;

                    // x
                    int x = 1; 
                    [|if (f)
                    {
                        x = 2;
                    }
                    else if (f)
                    {
                        x = 3;
                    }
                    else
                    {
                        x = 5;
                    }|]

                    return x; // 1
                }
            }
            """);

    [TestMethod]
    public Task WarnOnIfElseStatementWithUnnecessaryAssignmentThrowExceptionAtEnd() => VerifyAsync("""
                using System;
                
                class C
                {
                    int M()
                    {
                        bool f = false;
                
                        int x = 1;
                        [|if (f)
                        {
                            x = 2;
                        }
                        else if (f)
                        {
                            x = 3;
                        }
                        else
                        {
                            throw new Exception();
                        }|]
                
                        return x;
                    }
                }
                """);
    
    [TestMethod]
    public Task UnnecessaryAssignment() => VerifyAsync("""
        public class Test
        {
            int Number()
            {
                int x = 1;
                int y = 3;
                if (x > 1)
                {
                    x = 2;
                }
                else
                {
                    y = 4;
                }

                return x;
            }
        }
        """
    );

}
