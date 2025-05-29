# Example project to test linters

Seems like the modern way to do it (as of 2024) is to use roslyn analysers, that
work during build and as part of `dotnet format`.

This project uses built-in analysers and a few extras. See details below.

# Example: run in this project
```sh
# to see existing warnings/errors:
dotnet build --force --no-incremental
# TODO try this instead? dotnet format --verify-no-changes
# add linting settings to this project:
dotnet fsi add_linting.fsx .
# to see warnings/errors after adding linting:
dotnet build --force --no-incremental
# fix formatting issues:
dotnet format
# TODO try this
dotnet format style
```

# Quick start: add linting to your project
OVERWRITES YOUR PROJECT FILES! Make sure they're in source control.

This adds my opinionated settings to your project. You should review these.
If you've got a large solution with many csproj's, I recommend doing one at
a time, so you can tweak your linting preferences incrementally while avoiding
breaking the solution's build.

```sh
# see all warnings/errors before adding linting:
pushd <your project root>
dotnet build --force --no-incremental
popd
dotnet fsi add_linting.fsx <your project root> --edconfig
dotnet fsi add_linting.fsx <your project root>/proj1 --csproj
pushd <your project root>
# see all warnings/errors:
dotnet build --force --no-incremental
# auto-fix where possible:
dotnet format
# add linting to next project
dotnet fsi add_linting.fsx <your project root>/proj2 --csproj
# and so on...
```

# todo
- fix long lines: not part of built-in or added analysers :(
- remove redundant qualifiers on dotnet format
    - eg. unnecessary System in System.Console
- fix/find workarounds for quirks below
- add_linting script
    - add `dotnet_diagnostic.CA1707.severity = none` to test projects
        - non-root editorconfig
        - allows test_names_with_underscores

# quirks
`dotnet format` seems a little rough compared to `eslint`. Problems I've found
are below. Note these may be due to individual and/or conflicting analysers.

- some fixes take two runs to fix eg.
    - run 1: async void method changed to async task
    - run 2: fix non-awaiting caller (badly, see CS4014 below)
- some fixes break the build. eg.
    - `var asdf = new[] {new[] {1}, new[] {2}};` changes to
    - `var asdf = new[] {new[] [1], [2]}` <--- this does not compile
- CS4014 quirk: `dotnet fix` applies a discard instead of awaiting the call.
  Why? I want to await it. Example:
    - ```cs
      // before dotnet fix
      DoThingAsync();
      // after dotnet fix
      _ = DoThingAsync();
      // I want:
      await DoThingAsync();
      ```
    - see
        - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014
        - https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/discards
- make add_linting.fsx easier to tweak
- set stylecop warnings to errors. not possible? https://stackoverflow.com/questions/24804315/warnings-as-errors-does-not-apply-to-stylecop-warnings
- rider (IDE) sometimes shows warnings that have been disabled in .editorconfig
    - workaround: restart IDE

# interesting rules
- [CA1062](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1062)
  will still ping you if you've enabled nullable reference types. Why? Public
  methods may be called by clients who haven't enabled nullable refs: https://github.com/dotnet/roslyn-analyzers/issues/2875#issuecomment-536408486

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
