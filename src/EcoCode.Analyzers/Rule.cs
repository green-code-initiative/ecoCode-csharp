namespace EcoCode.Analyzers;

internal static class Rule
{
    public static string GetHelpUri(string id) =>
        $"https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/{id}/csharp/{id}.asciidoc";

    public static class Categories
    {
        public const string Design = "Design";
        public const string Usage = "Usage";
        public const string Performance = "Performance";
    }

    public static class Ids
    {
        public const string EC69_DontCallFunctionsInLoopConditions = "EC69";
        public const string EC72_DontExecuteSqlCommandsInLoops = "EC72";
        public const string EC75_DontConcatenateStringsInLoops = "EC75";
        public const string EC81_UseStructLayout = "EC81";
    }
}
