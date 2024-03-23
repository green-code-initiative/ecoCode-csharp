[![EcoCode on NuGet](https://img.shields.io/nuget/v/EcoCode.svg)](https://www.nuget.org/packages/EcoCode/) [![EcoCode on NuGet](https://img.shields.io/nuget/dt/EcoCode)](https://www.nuget.org/packages/EcoCode/)

EcoCode-C#
===========

_ecoCode_ is a collective project aiming to reduce environmental footprint of software at the code level. The goal of the project is to provide a list of static code analyzers to highlight code structures that may have a negative ecological impact: energy and resources over-consumption, "fatware", shortening terminals' lifespan, etc.

_ecoCode_ is based on evolving catalogs of [good practices](https://github.com/green-code-initiative/ecoCode/blob/main/docs/rules), for various technologies.

This set of Roslyn analyzers implements these catalogs as rules for scanning your C# projects.

> ⚠️ This is still a very early stage project. Any feedback or contribution will be highly appreciated. Please refer to the contribution section.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](https://github.com/green-code-initiative/ecoCode-common/blob/main/doc/CODE_OF_CONDUCT.md)

🚀 Getting Started
------------------

Reference the EcoCode NuGet package in your .Net projects : https://www.nuget.org/packages/EcoCode/, and make sure analyzers are not disabled.

🧩 Compatibility
-----------------

The EcoCode NuGet package targets .Net Standard 2.0 and can be used in a wide range of projects. See [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version) for details about the supported .Net Frameworks in .Net Standard 2.0.

🌿 Rules
-------------------

|Id|Description|Severity|Enabled|Code fix|
|--|-----------|:------:|:--------:|:------:|
|[EC69](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC69/csharp/EC69.asciidoc)|Don’t call loop invariant functions in loop conditions|⚠️|✔️|❌|
|[EC72](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC72/csharp/EC72.asciidoc)|Don’t execute SQL queries in loops|⚠️|✔️|❌|
|[EC75](https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/EC75/csharp/EC75.asciidoc)|Don’t concatenate `strings` in loops|⚠️|✔️|❌|

🤝 Contribution
---------------

See [contribution](https://github.com/green-code-initiative/ecoCode#-contribution) on the central ecoCode repository.

🤓 Main contributors
--------------------

See [main contributors](https://github.com/green-code-initiative/ecoCode#-main-contributors) on the central ecoCode repository.
