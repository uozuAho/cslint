# Example project to test linters


# Add linting to your solution
```sh
# run this. It assumes your solution doesn't contain existing linting resources.
./install_linting.sh

# try it out
cd <your solution>
./lint.sh check
./lint.sh fix
```

For a large solution with many projects, you may need to temporarily
remove/tweak/comment out `Directory.Build.props` to un-break the build while you
are fixing all linting issues. I've configured the strictest possible
properties, which is overwhelming when adding to an existing project of
non-trivial size. It's probably a better approach to introduces small sets of
rules at a time.

See
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options
- https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props


# quirks
`dotnet format` seems a little rough compared to `eslint`. Problems I've found
are below. Note these may be due to individual and/or conflicting analysers.

- doesn't do basic formatting! Argh!
    - wrapping long lines, unnecessary whitespace, tabs v spaces
    - workaround used: use https://csharpier.com
- some `dotnet format` fixes take two runs to fix eg.
    - run 1: async void method changed to async task
    - run 2: fix non-awaiting caller (badly, see CS4014 below)
    - watch this happen in `myapp.Program.SomeSyncMethod`
- some fixes may break builds/tests
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
- some rules (eg. CA1852, CA1724) doesn't show up in rider, even after multiple
  restarts, and don't get fixed by `dotnet format`.

# interesting rules
- [CA1062](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1062)
  will still ping you if you've enabled nullable reference types. Why? Public
  methods may be called by clients who haven't enabled nullable refs: https://github.com/dotnet/roslyn-analyzers/issues/2875#issuecomment-536408486


--------------------------------------------------------------

# Formatter / linter options
## .NET analysers
It seems like the modern way to do it (as of 2024) is to use roslyn analysers,
that work during build and as part of `dotnet format`.

See https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8

These mostly work, integrate with many IDEs, and are available without any extra
packages (although you may want to add more analysers to support your style
requirements).

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


## CSharpier
`dotnet format` misses a bunch of basic formatting that is sorely needed IMO -
long lines, unnecessary whitespace, tabs v spaces etc.

[CSharpier](https://csharpier.com/) does just this. Phew.


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
