using System.Runtime.InteropServices;

namespace EcoCode.Tests;

internal static class SpecifyStructLayoutLiveWarnings
{
    private record struct TestStruct1;

    private record struct TestStruct2(int A);

    private record struct TestStruct3(string A);

    private record struct TestStruct4(int A, double B); // EC81 - Use struct layout

    [StructLayout(LayoutKind.Auto)]
    private record struct TestStruct4_Fixed(int A, double B);

    private record struct TestStruct5(int A, string B);

    private record struct TestStruct6(int A, double B, int C); // EC81 - Use struct layout

    [StructLayout(LayoutKind.Auto)]
    private record struct TestStruct6_Fixed(int A, double B, int C);

    private record struct TestStruct7(bool A, int B, char C, short D, ulong E, DateTime F); // EC81 - Use struct layout

    [StructLayout(LayoutKind.Auto)]
    private record struct TestStruct7_Fixed(bool A, int B, char C, short D, ulong E, DateTime F);
}
