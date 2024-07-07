using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Composition;

namespace EcoCode.Vsix;

/// <summary>Provide options from an analyzer config file keyed on a source file.</summary>
[Shared]
[Export(typeof(AnalyzerConfigOptionsProvider))]
public sealed class EcoCodeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    /// <inheritdoc/>
    public override AnalyzerConfigOptions GlobalOptions => EcoCodeAnalyzerConfigOptions.Default;

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => EcoCodeAnalyzerConfigOptions.Default;

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => EcoCodeAnalyzerConfigOptions.Default;

    private sealed class EcoCodeAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        public static EcoCodeAnalyzerConfigOptions Default { get; } = new();

        private EcoCodeAnalyzerConfigOptions()
        {
        }

        public override bool TryGetValue(string key, out string value) => Options.TryGetValue(key, out value);

        private static readonly Dictionary<string, string> Options = new()
        {
            { "dotnet_diagnostic.CA1001.severity", "warning" }, // Types that own disposable fields should be disposable
            { "dotnet_diagnostic.CA1802.severity", "warning" }, // Use Literals Where Appropriate
            { "dotnet_diagnostic.CA1805.severity", "warning" }, // Do not initialize unnecessarily
            { "dotnet_diagnostic.CA1813.severity", "warning" }, // Avoid unsealed attributes
            { "dotnet_diagnostic.CA1816.severity", "warning" }, // Call GC.SuppressFinalize correctly
            { "dotnet_diagnostic.CA1821.severity", "warning" }, // Remove empty finalizers
            { "dotnet_diagnostic.CA1822.severity", "warning" }, // Mark members as static
            { "dotnet_diagnostic.CA1824.severity", "warning" }, // Mark assemblies with NeutralResourcesLanguageAttribute
            { "dotnet_diagnostic.CA1825.severity", "warning" }, // Avoid zero-length array allocations
            { "dotnet_diagnostic.CA1826.severity", "warning" }, // Use property instead of Linq Enumerable method
            { "dotnet_diagnostic.CA1827.severity", "warning" }, // Do not use Count()/LongCount() when Any() can be used
            { "dotnet_diagnostic.CA1828.severity", "warning" }, // Do not use CountAsync/LongCountAsync when AnyAsync can be used
            { "dotnet_diagnostic.CA1829.severity", "warning" }, // Use Length/Count property instead of Enumerable.Count method
            { "dotnet_diagnostic.CA1830.severity", "warning" }, // Prefer strongly-typed Append and Insert method overloads on StringBuilder
            { "dotnet_diagnostic.CA1832.severity", "warning" }, // Use AsSpan or AsMemory instead of Range-based indexers for getting ReadOnlySpan or ReadOnlyMemory portion of an array
            { "dotnet_diagnostic.CA1833.severity", "warning" }, // Use AsSpan or AsMemory instead of Range-based indexers for getting Span or Memory portion of an array
            { "dotnet_diagnostic.CA1834.severity", "warning" }, // Prefer StringBuilder.Append(char) for single character strings
            { "dotnet_diagnostic.CA1835.severity", "warning" }, // Prefer the memory-based overloads of Read
            { "dotnet_diagnostic.CA1836.severity", "warning" }, // Prefer IsEmpty over Count when available
            { "dotnet_diagnostic.CA1837.severity", "warning" }, // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
            { "dotnet_diagnostic.CA1838.severity", "warning" }, // Avoid StringBuilder parameters for P/Invokes
            { "dotnet_diagnostic.CA1839.severity", "warning" }, // Use Environment.ProcessPath instead of Process.GetCurrentProcess().MainModule.FileName
            { "dotnet_diagnostic.CA1840.severity", "warning" }, // Use Environment.CurrentManagedThreadId instead of Thread.CurrentThread.ManagedThreadId
            { "dotnet_diagnostic.CA1841.severity", "warning" }, // Prefer Dictionary Contains methods
            { "dotnet_diagnostic.CA1842.severity", "warning" }, // Do not use 'WhenAll' with a single task
            { "dotnet_diagnostic.CA1843.severity", "warning" }, // Do not use 'WaitAll' with a single task
            { "dotnet_diagnostic.CA1844.severity", "warning" }, // Provide memory-based overrides of async methods when subclassing 'Stream'
            { "dotnet_diagnostic.CA1845.severity", "warning" }, // Use span-based 'string.Concat'
            { "dotnet_diagnostic.CA1846.severity", "warning" }, // Prefer AsSpan over Substring
            { "dotnet_diagnostic.CA1847.severity", "warning" }, // Use String.Contains(char) instead of String.Contains(string) with single characters
            { "dotnet_diagnostic.CA1850.severity", "warning" }, // Prefer static HashData method over ComputeHash
            { "dotnet_diagnostic.CA1853.severity", "warning" }, // Unnecessary call to 'Dictionary.ContainsKey(key)'
            { "dotnet_diagnostic.CA1854.severity", "warning" }, // Prefer the IDictionary.TryGetValue(TKey, out TValue) method
            { "dotnet_diagnostic.CA1855.severity", "warning" }, // Use Span<T>.Clear() instead of Span<T>.Fill()
            { "dotnet_diagnostic.CA1858.severity", "warning" }, // Use StartsWith instead of IndexOf
            { "dotnet_diagnostic.CA1859.severity", "warning" }, // Use concrete types when possible
            { "dotnet_diagnostic.CA1860.severity", "warning" }, // Avoid using 'Enumerable.Any()'
            { "dotnet_diagnostic.CA1863.severity", "warning" }, // Use 'CompositeFormat'
            { "dotnet_diagnostic.CA1864.severity", "warning" }, // Prefer the 'IDictionary.TryAdd(TKey, TValue)' method
            { "dotnet_diagnostic.CA1865.severity", "warning" }, // Use 'string.Method(char)' instead of 'string.Method(string)' for string with single char
            { "dotnet_diagnostic.CA1866.severity", "warning" }, // Use 'string.Method(char)' instead of 'string.Method(string)' for string with single char
            { "dotnet_diagnostic.CA1867.severity", "warning" }, // Use 'string.Method(char)' instead of 'string.Method(string)' for string with single char
            { "dotnet_diagnostic.CA1868.severity", "warning" }, // Unnecessary call to 'Contains' for sets
            { "dotnet_diagnostic.CA1869.severity", "warning" }, // Cache and reuse 'JsonSerializerOptions' instances
            { "dotnet_diagnostic.CA1870.severity", "warning" }, // Use a cached 'SearchValues' instance
            { "dotnet_diagnostic.CA1871.severity", "warning" }, // Do not pass a nullable struct to 'ArgumentNullException.ThrowIfNull'
            { "dotnet_diagnostic.CA1872.severity", "warning" }, // Prefer 'Convert.ToHexString' and 'Convert.ToHexStringLower' over call chains based on 'BitConverter.ToString'
            { "dotnet_diagnostic.CA2009.severity", "warning" }, // Do not call ToImmutableCollection on an ImmutableCollection value
            { "dotnet_diagnostic.CA2215.severity", "warning" }, // Dispose methods should call base class dispose
            { "dotnet_diagnostic.CA2218.severity", "warning" }, // Override GetHashCode on overriding Equals
            { "dotnet_diagnostic.CA2251.severity", "warning" }, // Use String.Equals over String.Compare
            { "dotnet_diagnostic.CA2264.severity", "warning" }, // Do not pass a non-nullable value to 'ArgumentNullException.ThrowIfNull'
        };
    }
}
