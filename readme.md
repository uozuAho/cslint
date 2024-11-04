# Example project to test linters

None of the problems in the code are picked up by `dotnet run`

# todo
- read https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8
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
