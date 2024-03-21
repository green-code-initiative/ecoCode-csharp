namespace EcoCode.Tests;

[TestClass]
public class DontExecuteSqlCommandsInLoopsUnitTests
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        DontExecuteSqlCommandsInLoops,
        DontExecuteSqlCommandsInLoopsCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task DontExecuteSqlCommandsInLoopsAsync() => await VerifyAsync("""
        using System.Data;
        public class Test
        {
            public void Run(int p)
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
