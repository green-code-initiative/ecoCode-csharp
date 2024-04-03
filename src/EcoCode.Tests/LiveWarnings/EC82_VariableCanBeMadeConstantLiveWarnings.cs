namespace EcoCode.Tests;

internal static class VariableCanBeMadeConstantLiveWarnings
{
    public static void Test1()
    {
        int i = 0; // EC82: Variable can be made constant
        Console.WriteLine(i);

        int j = 0;
        Console.WriteLine(j++);

        const int k = 0;
        Console.WriteLine(k);

        int m = DateTime.Now.DayOfYear;
        Console.WriteLine(m);

        int n = 0, o = 0; // EC82: Variable can be made constant
        Console.WriteLine(n);
        Console.WriteLine(o);

        object p = "abc";
        Console.WriteLine(p);

        string q = "abc"; // EC82: Variable can be made constant
        Console.WriteLine(q);

        var r = 4; // EC82: Variable can be made constant
        Console.WriteLine(r);

        var s = "abc"; // EC82: Variable can be made constant
        Console.WriteLine(s);
    }
}
