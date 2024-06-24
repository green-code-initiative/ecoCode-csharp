[![EcoCode on NuGet](https://img.shields.io/nuget/v/EcoCode.svg)](https://www.nuget.org/packages/EcoCode/) [![EcoCode on NuGet](https://img.shields.io/nuget/dt/EcoCode)](https://www.nuget.org/packages/EcoCode/)

EcoCode-C#
===========

_ecoCode_ is a collective project aiming to reduce environmental footprint of software at the code level. The goal of the project is to provide a list of static code analyzers to highlight code structures that may have a negative ecological impact: energy and resources over-consumption, "fatware", shortening terminals' lifespan, etc.

_ecoCode_ is based on evolving catalogs of [good practices](https://github.com/green-code-initiative/ecoCode/blob/main/docs/rules), for various technologies. This set of Roslyn analyzers implements these catalogs as rules for scanning your C# projects.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](https://github.com/green-code-initiative/ecoCode-common/blob/main/doc/CODE_OF_CONDUCT.md)

🚀 Getting Started
------------------

There are several ways you can use the ecoCode C# analyzers in your .Net projects:
1. As a [NuGet package](#nugetPackage)
2. As a [.NET tool](#dotnetTool)
3. As a [Visual Studio extension](#vsExtension)
4. Coming soon : as a VS Code extension.
5. As an [analyzer for SonarQube](#sonarQube).

<a name="nugetPackage"></a>🧩 NuGet package
-----------------
The package is available on nuget.org at this address : https://www.nuget.org/packages/EcoCode, and can be added to your projects/solutions like any NuGet package. Once referenced and restored, the ecoCode analyzers are automatically integrated in your IDE/compilation process, and will list any applicable info/alert.

Pre-requisite : .Net Standard 2.0, which can be used in a wide range of projects. See [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version) for details about the supported Frameworks.

<a name="dotnetTool"></a>🧩 .Net tool
-----------------
The .Net tool is available on nuget.org at this address : https://www.nuget.org/packages/EcoCode.Tool, and can be fetched on your machine using the following command :

`dotnet tool install --global EcoCode.Tool`

See [.Net tools documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) for additional information.

Once installed, you can launch an analyzis on an existing codebase like this :

`ecocode-cli analyze path/to/mySolution.sln path/to/myReport.html`.

The file to analyze can be a .sln, a .slnx or a .csproj. The report format depends on it's required extension, the following are currently supported : .html, .json and .csv.

Pre-requisite : .Net 8 SDK, with the goal of requiring the oldest .Net Core LTS from then on (ie. require .Net 10 in November 2026).

<a name="vsExtension"></a>🧩 Visual Studio extension
-----------------
The extension is available on  the VS marketplace at this address : https://marketplace.visualstudio.com/items?itemName=greencodeinitiative.ecoCode, and can be installed in your Visual Studio instance like any extension, typically through the extension manager available in Visual Studio. Once installed, the ecoCode analyzers are automatically activated in your IDE/compilation process, and will list any applicable info/alert.

<a name="vsCodeExtension"></a>🧩 VS Code extension
-----------------
Not available yet, stay tuned.

<a name="sonarQube"></a>🧩 Analyzer for SonarQube
-----------------
EcoCode C# can use [SonarScanner for .Net](https://docs.sonarsource.com/sonarqube/latest/analyzing-source-code/scanners/sonarscanner-for-dotnet/) to integrate with [SonarQube](https://www.sonarsource.com/products/sonarqube/), and uses a custom import addition to enrich what is reported to Sonar (severity, description, url page, category, and so on). See our [dedicated repository](https://github.com/green-code-initiative/ecoCode-csharp-sonarqube) for more information.

🌿 EcoCode Rules
-------------------

|Id|Description|Severity|Code fix|
|--|-----------|:------:|:------:|
|[EC69](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC69/csharp/EC69.asciidoc)|Don’t call loop invariant functions in loop conditions|⚠️|❌|
|[EC72](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC72/csharp/EC72.asciidoc)|Don’t execute SQL queries in loops|⚠️|❌|
|[EC75](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC75/csharp/EC75.asciidoc)|Don’t concatenate `strings` in loops|⚠️|❌|
|[EC81](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC81/csharp/EC81.asciidoc)|Specify `struct` layouts|⚠️|✔️|
|[EC82](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC82/csharp/EC82.asciidoc)|Variable can be made constant|ℹ️|✔️|
|[EC83](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC83/csharp/EC83.asciidoc)|Replace Enum `ToString()` with `nameof`|⚠️|✔️|
|[EC84](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC84/csharp/EC84.asciidoc)|Avoid `async void` methods|⚠️|✔️|
|[EC85](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC85/csharp/EC85.asciidoc)|Make type `sealed`|ℹ️|✔️|
|[EC86](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC86/csharp/EC86.asciidoc)|`GC.Collect` should not be called|⚠️|❌|
|[EC87](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC87/csharp/EC87.asciidoc)|Use collection indexer|⚠️|✔️|
|[EC88](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC88/csharp/EC88.asciidoc)|Dispose resource asynchronously|⚠️|✔️|
|[EC91](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC91/csharp/EC91.asciidoc)|Use `Where` before `OrderBy`|⚠️|✔️|
|[EC92](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC92/csharp/EC92.asciidoc)|Use `Length` to test empty `strings`|⚠️|✔️|
|[EC93](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC93/csharp/EC93.asciidoc)|Return `Task` directly|ℹ️|✔️|

🌿 Roslyn Rules
-------------------

This plugin customizes the severity of certain Roslyn rules.

|Id|Description|Old Severity|New Severity|
|--|-----------|:----------:|:----------:|
|[CA1825](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1825)|Avoid zero-length array allocations|ℹ️|⚠️|
|[CA1827](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1827)|Do not use Count()/LongCount() when Any() can be used|ℹ️|⚠️|

🤝 Contribution
---------------

See [contribution](https://github.com/green-code-initiative/ecoCode#-contribution) on the central ecoCode repository.

🤓 Main contributors
--------------------

See [main contributors](https://github.com/green-code-initiative/ecoCode#-main-contributors) on the central ecoCode repository.
