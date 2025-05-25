# Example project to test linters

Seems like the modern way to do it (as of 2024) is to use roslyn analysers, that
work during build and as part of `dotnet format`.

This project uses built-in analysers and a few extras. See details below.

```sh
# to see warnings/errors:
dotnet build
# to fix:
dotnet format
```

# todo
- fsi
    - add editorconfig elements
        <!-- [*.cs]
        dotnet_analyzer_diagnostic.category-Style.severity = error
        dotnet_diagnostic.IDE0008.severity = none     # prefer var to int
        dotnet_diagnostic.CA2007.severity = none      # avoid ConfigureAwait everywhere (see https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2007)
        dotnet_diagnostic.VSTHRD100.severity = error  # never use async void (require in-code override)

        dotnet_diagnostic.SA0001.severity = none      # XML comment analysis
        dotnet_diagnostic.SA1300.severity = none      # cslint should begin with caps
        dotnet_diagnostic.SA1633.severity = none      # project file header -->
- fix long lines: not part of built-in or added analysers :(
- prevent missing awaits. CS4014. Should be caught, why isn't it?
- fix "GenerateDocumentationFile to enable IDE0005" warning without needing XML
  docs
    - note that IDE0005 works without GenerateDocumentationFile
- set stylecop warnings to errors. not possible? https://stackoverflow.com/questions/24804315/warnings-as-errors-does-not-apply-to-stylecop-warnings

# Options
## Built-in analysers
See https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8

These work well, and are available without any extra packages (although you may
want to add more analysers to support your style requirements).

When configured as below, they run during build, and many violations can be
fixed by running `dotnet format`.

Quick start to add to your projects:

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

### Built-in rules
- quality (CA*): https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/
- style (IDE*): https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/

### Analysers added to this project
- [Microsoft.VisualStudio.Threading.Analyzers](https://github.com/microsoft/vs-threading/blob/main/doc/analyzers/index.md):
    - naming: Async suffix, prevent async void, more
    - doesn't catch missing awaits
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
    - whitespace, regions, more

### Other interesting analysers:
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
