namespace EcoCode.Analyzers;

internal static class Rule
{
    public static string GetHelpUri(string id) =>
        $"https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/{id}/cs/{id}.asciidoc";

    public static class Categories
    {
        public const string Design = "Design";
        public const string Usage = "Usage";
        public const string Performance = "Performance";
    }

    public static class Ids
    {
        public const string DontCallFunctionsInLoopCondition = "EC69";
        public const string DontExecuteSqlCommandsInLoops = "EC72";
        public const string DontConcatenateStringsInLoops = "EC75";
    }
}