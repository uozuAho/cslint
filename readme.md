# Example project to test linters

None of the problems in the code are picked up by `dotnet run`

# todo
## built-in analysers
- prevent missing awaits
- get rid of unnecessary whitespace
- disallow regions
- fix "GenerateDocumentationFile to enable IDE0005" warning without needing XML
  docs
    - note that IDE0005 works without GenerateDocumentationFile
## maybe
- read https://www.meziantou.net/the-roslyn-analyzers-i-use.htm
- try
    - https://github.com/dotnet/roslyn-analyzers
    - Microsoft.CodeAnalysis.FxCopAnalyzers
    - https://github.com/DotNetAnalyzers/StyleCopAnalyzers
- try roslynator again

# What I want in a linter
- cli
- naming rules: eg. async suffix
- whitespace, brackets
- autofix
- ide integration
- ability to ignore rules in code eg. //ignoreNextLine
- ability to ignore all analysis (eg. gradual integration, need to get a build
  out fast)
- maybe
    - custom rules?

# Options
## Built-in analysers
See https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8

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

Interesting analysers:
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
