EcoCode-C#
===========

_ecoCode_ is a collective project aiming to reduce environmental footprint of software at the code level. The goal of the project is to provide a list of static code analyzers to highlight code structures that may have a negative ecological impact: energy and resources over-consumption, "fatware", shortening terminals' lifespan, etc.

_ecoCode_ is based on evolving catalogs of [good practices](https://github.com/green-code-initiative/ecoCode/blob/main/docs/rules), for various technologies.

This set of Roslyn analyzers implements these catalogs as rules for scanning your C# projects.

> ⚠️ This is still a very early stage project. Any feedback or contribution will be highly appreciated. Please refer to the contribution section.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](https://github.com/green-code-initiative/ecoCode-common/blob/main/doc/CODE_OF_CONDUCT.md)

🌿 Framework
-------------------

This analyzer is made to be published as NuGet package, and is compiled using the .Net Standard 2.0 Framework.
You can find a list of all our other plugins in the [ecoCode repository](https://github.com/green-code-initiative/ecoCode#-sonarqube-plugins)

🚀 Getting Started
------------------

Provide link to NuGet package. Simply install the package in your project and let the analysis run.

🛒 Distribution
------------------

Ready to use binaries are available [from GitHub](https://github.com/green-code-initiative/ecoCode-java/releases).

🧩 Compatibility
-----------------

See Microsoft documentation for [.Net Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version).

| .NET implementation        | Version support                             |
| :------------------------: | :-----------------------------------------: |
| .NET and .NET Core         | 2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0, 7.0, 8.0 |
| .NET Framework             | 4.6.1, 4.6.2, 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1 |
| Mono						 | 5.4, 6.4                                    |
| Xamarin.iOS				 | 10.14, 12.16                                |
| Xamarin.Mac				 | 3.8, 5.16                                   |
| Xamarin.Android			 | 8.0, 10.0                                   |
| Universal Windows Platform | 10.0.16299, TBD                             |
| Unity						 | 2018.1                                      |

🤝 Contribution
---------------

Check [ecoCode repository](https://github.com/green-code-initiative/ecoCode#-contribution)

🤓 Main contributors
--------------------

Check [ecoCode repository](https://github.com/green-code-initiative/ecoCode#-main-contributors)
