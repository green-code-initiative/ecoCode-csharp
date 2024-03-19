using Verifier = EcoCode.Tests.CodeFixVerifier<
    EcoCode.Analyzers.DontExecuteSqlCommandsInLoops,
    EcoCode.CodeFixes.DontExecuteSqlCommandsInLoopsCodeFixProvider>;

namespace EcoCode.Tests;

[TestClass]
public class DontExecuteSqlCommandsInLoopsUnitTests
{
    [TestMethod]
    public async Task EmptyCodeAsync() => await Verifier.VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task DontExecuteSqlCommandsInLoopsAsync() => await Verifier.VerifyAsync("""
        using System.Data;
        public class Test
        {
            public void Run()
            {
                var command = default(IDbCommand)!;
                _ = command.ExecuteNonQuery();
                _ = command.ExecuteScalar();
                _ = command.ExecuteReader();
                _ = command.ExecuteReader(CommandBehavior.Default);

                for (int i = 0; i < 10; i++)
                {
                    _ = [|command.ExecuteNonQuery()|];
                    _ = [|command.ExecuteScalar()|];
                    _ = [|command.ExecuteReader()|];
                    _ = [|command.ExecuteReader(CommandBehavior.Default)|];
                }
            }
        }
        """).ConfigureAwait(false);
}