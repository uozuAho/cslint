# Example project to test linters

None of the problems in the code are picked up by `dotnet run`

# todo
- set stylecop warnings to errors
- prevent missing awaits
- fix "GenerateDocumentationFile to enable IDE0005" warning without needing XML
  docs
    - note that IDE0005 works without GenerateDocumentationFile
- check IDE integration
- how much time does this add to builds?

# Options
## Built-in analysers
See https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8

These work well, and are available without any extra packages (although you may
want to add more analysers to support your style requirements).

When configured as below, they run during build, and many violations can be
fixed by running `dotnet format`.

Quick start:

- enable all rules: add `<AnalysisMode>All</AnalysisMode>` to your project file
- treat warnings as errors:
  `<CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>`
- check code style during build: add
  `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to your project file
- treat all style warnings as errors: add
  `dotnet_analyzer_diagnostic.category-Style.severity = error` to your
  editorconfig

All checks are done during `dotnet build|run`
Run `dotnet format` to autoformat.

Analysers added to this project
- Microsoft.VisualStudio.Threading.Analyzers:
    - naming: Async suffix, prevent async void, more
    - doesn't catch missing awaits
- StyleCop.Analyzers
    - whitespace, regions, more

Other interesting analysers:
- Microsoft.CodeAnalysis.BannedApiAnalyzers
- Microsoft.CodeAnalysis.PublicApiAnalyzers

## Roslynator
I can't get this to work properly:
- install & analyse works
- nothing in editorconfig seems to be used
- doesn't analyse on build (do I want this?)

- https://josefpihrt.github.io/docs/roslynator/
- quick start (not currently in docs):
```sh
dotnet tool install -g roslynator.dotnet.cli
dotnet add package roslynator.analyzers
roslynator analyze
```
