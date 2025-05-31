# Example project to test linters

Seems like the modern way to do it (as of 2024) is to use roslyn analysers, that
work during build and as part of `dotnet format`.

This project uses built-in analysers and a few extras. See details below.

# Example: run in this project
```sh
# to see existing warnings/errors:
dotnet format --verify-no-changes
# add linting settings to this project:
dotnet fsi add_linting.fsx .
# to see warnings/errors after adding linting:
./lint_and_format.sh check
# auto-fix where possible. You may need to do some manual fixes.
./lint_and_format.sh fix
```

# Quick start: add linting to your project
MODIFIES YOUR PROJECT FILES! Make sure they're in source control.

This adds my opinionated settings to your project. You should review these.
If you've got a large solution with many csproj's, I recommend doing one at
a time, so you can tweak your linting preferences incrementally while avoiding
breaking the solution's build.

```sh
# see all warnings/errors before adding linting:
pushd <your project root>
dotnet build --force --no-incremental
popd
dotnet fsi add_linting.fsx <your project root> --edconfig --csharpier
dotnet fsi add_linting.fsx <your project root>/proj1 --csproj
./lint_and_format.sh check
./lint_and_format.sh fix
# add linting to next project
dotnet fsi add_linting.fsx <your project root>/proj2 --csproj
./lint_and_format.sh check
./lint_and_format.sh fix
# ..and so on...
```

# todo
- https://csharpier.com/
    - DONE add to add_linting script
    - DONE try it
    - DONE add lint and format script
    - DONE add help msg to end of add_linting script
    - clean linting script
        - use run for other cmd calls
        - use result type everywhere
    - add more docs
        - why csharpier
- make add_linting.fsx easier to tweak
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
    - watch this happen in `myapp.Program.SomeSyncMethod`
- some fixes break the build. eg.
    - `var asdf = new[] {new[] {1}, new[] {2}};` changes to
    - `var asdf = new[] {new[] [1], [2]}` <--- this does not compile
    - **todo** is this still happening after removing stylecop?
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
- rider (IDE) sometimes shows warnings that have been disabled in .editorconfig
    - workaround: restart IDE

# interesting rules
- [CA1062](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1062)
  will still ping you if you've enabled nullable reference types. Why? Public
  methods may be called by clients who haven't enabled nullable refs: https://github.com/dotnet/roslyn-analyzers/issues/2875#issuecomment-536408486


--------------------------------------------------------------

# Formatter / linter options
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

### Analysers not added to this project
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
    - cleans up unnecessary whitespace, regions, namespace naming
    - bit more pedantic than built in rules
    - unfortunately, seems a little out of date, and conflicts with some built-
      in rules. Some examples:
        - doesn't like uppercase parameter names in record primary constructors

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
