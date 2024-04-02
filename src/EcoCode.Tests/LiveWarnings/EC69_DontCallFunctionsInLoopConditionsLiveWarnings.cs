namespace EcoCode.Tests.LiveWarnings;

internal static class DontCallFunctionsInLoopConditionsLiveWarnings
{
    private const int C = 10;
    private static int V1 => 10;
    private static int V2() => 10;
    private static int V3(int i) => i;

    public static void Run(int p)
    {
        int j = 0, k = 10;

        for (int i = 0; i < V1 && i < V2() && i < V3(i) && i < V3(j) && i < V3(k) && i < V3(p) && i < V3(C); i++)
            j += i;

        for (int i = 0; i < V1 && i < V2() && i < V3(i) && i < V3(j) && i < V3(k) && i < V3(p) && i < V3(C); i++)
        {
            j += i;
        }
    }
}
