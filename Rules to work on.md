Ongoing existing rules list that could be analyzed/implemented in our C# project.
They seem to be good candidates but need reviewing first.

From [Roslynator](https://github.com/dotnet/roslynator):
+ Optimize LINQ method call: [RCS1077](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1077/)
+ Mark local variable as const: [RCS1118](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1118/)
+ Use string.Length instead of comparison with empty string: [RCS1156](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1156/)
+ Use 'is' operator instead of 'as' operator: [RCS1172](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1172/)
+ Unnecessary assignment: [RCS1179](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1179/)
+ Use constant instead of field : [RCS1187](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1187/)
+ Optimize StringBuilder.Append/AppendLine call : [RCS1197](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1197/)
+ Avoid unnecessary boxing of value type : [RCS1198](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1198/)
+ Optimize method call : [RCS1235](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1235/)
+ Do not pass non-read-only struct by read-only reference : [RCS1242](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1242/)

From [Meziantou Analyzer](https://github.com/meziantou/Meziantou.Analyzer):
+ Use Array.Empty<T>(): [MA0005](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0005.md)
+ Use EventArgs.Empty: [MA0019](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0019.md)
+ Use direct methods instead of LINQ methods: [MA0020](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0020.md)
+ Optimize StringBuilder usage: [MA0028](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0028.md)
+ Combine LINQ methods: [MA0029](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0029.md)
+ Optimize Enumerable.Count() usage: [MA0031](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0031.md)
+ Do not use blocking calls in an async method: [MA0042](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0042.md)
+ Do not use finalizer: [MA0055](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0055.md)
+ Use Where before OrderBy: [MA0063](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0063.md)
+ Default ValueType.Equals or HashCode is used for struct equality: [MA0065](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0065.md)
+ Use 'Cast' instead of 'Select' to cast: [MA0078](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0078.md)
+ Optimize string method usage: [MA0089](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0089.md)
+ Use the Regex source generator: [MA0110](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0110.md)

From [SonarQube](https://www.sonarsource.com/products/sonarqube/):
+ Private fields only used as local variables in methods should become local variables: [S1450](https://cloud-ci.sgs.com/sonar/coding_rules?open=csharpsquid%3AS1450&amp;rule_key=csharpsquid%3AS1450)
+ Properties should not make collection or array copies: [S2365](https://cloud-ci.sgs.com/sonar/coding_rules?open=csharpsquid%3AS2365&amp;rule_key=csharpsquid%3AS2365)
+ "StringBuilder" data should be used: [S3063](https://cloud-ci.sgs.com/sonar/coding_rules?open=csharpsquid%3AS3063&amp;rule_key=csharpsquid%3AS3063)
+ "Assembly.GetExecutingAssembly" should not be called: [S3902](https://cloud-ci.sgs.com/sonar/coding_rules?open=csharpsquid%3AS3902&amp;rule_key=csharpsquid%3AS3902)
+ "static readonly" constants should be "const" instead: [S3962](https://cloud-ci.sgs.com/sonar/coding_rules?open=csharpsquid%3AS3962&amp;rule_key=csharpsquid%3AS3962)
+ Non-abstract attributes should be sealed: [S4060](https://cloud-ci.sgs.com/sonar/coding_rules?open=csharpsquid%3AS4060&amp;rule_key=csharpsquid%3AS4060)

Rules that are natively implemented in [Roslyn](https://github.com/dotnet/roslyn), that we could transitively enable:
+ Do not declare static members on generic types: [CA1000](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1000)
+ Mark members as static: [CA1822](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1822)
+ Use 'Count/Length' property instead of 'Any' method: [CA1860](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1860)
