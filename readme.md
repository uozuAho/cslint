# Example project to test linters

Seems like the modern way to do it (as of 2024) is to use roslyn analysers, that
work during build and as part of `dotnet format`.

This project uses built-in analysers and a few extras. See details below.

# Example: run in this project
```sh
# to see existing warnings/errors:
dotnet build --force --no-incremental
# add linting settings to this project:
dotnet fsi add_linting.fsx .
# to see warnings/errors after adding linting:
dotnet build --force --no-incremental
# fix formatting issues:
dotnet format
```

# Quick start: add linting to your project
OVERWRITES YOUR PROJECT FILES! Make sure they're in source control.

```sh
# see all warnings/errors before adding linting:
pushd <your project root>
dotnet build --force --no-incremental
popd
dotnet fsi add_linting.fsx <your project root>
pushd <your project root>
# see all warnings/errors:
dotnet build --force --no-incremental
# auto-fix where possible:
dotnet format
```

# todo
- get rid of cslint specific stuff in editorconfig
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

Using the settings added by `add_linting.fsx`, they run during build, and many
violations can be fixed by running `dotnet format`.

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
