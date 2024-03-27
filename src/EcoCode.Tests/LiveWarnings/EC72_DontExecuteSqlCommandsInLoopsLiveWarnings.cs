using System.Data;

namespace EcoCode.Tests;

internal static class DontExecuteSqlCommandsInLoopsLiveWarnings
{
    public static void Run()
    {
        var command = default(IDbCommand)!;
        _ = command.ExecuteNonQuery();
        _ = command.ExecuteScalar();
        _ = command.ExecuteReader();
        _ = command.ExecuteReader(CommandBehavior.Default);

        for (int i = 0; i < 10; i++)
        {
            _ = command.ExecuteNonQuery();
            _ = command.ExecuteScalar();
            _ = command.ExecuteReader();
            _ = command.ExecuteReader(CommandBehavior.Default);
        }
    }
}
