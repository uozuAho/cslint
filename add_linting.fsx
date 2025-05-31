open System.IO
open System.Text
open System.Xml
open System.Xml.Linq
open System
open System.Diagnostics


let findAllCsprojFiles (directory: string) =
    Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories)


let getElement (parent: XElement) (name: string) =
    match parent.Element(XName.Get name) with
    | null -> Error $"{parent.Name}.{name} not found"
    | el -> Ok el


let ensureElement (parent: XElement) (name: string) (value: string) =
    match parent.Element(XName.Get name) with
    | null ->
        let added = XElement(XName.Get name, value)
        parent.Add added
    | elem -> elem.Value <- value


let saveCsProj (filePath: string) (doc: XDocument) =
    let settings = XmlWriterSettings()
    settings.Indent <- true
    settings.OmitXmlDeclaration <- true
    settings.Encoding <- Encoding.UTF8

    use writer = XmlWriter.Create(filePath, settings)
    doc.Save(writer)


let addLintSettings (csProjFile: string) =
    let doc = XDocument.Load csProjFile

    match getElement doc.Root "PropertyGroup" with
    | Error msg -> raise (Exception $"In {csProjFile}: {msg}")
    | Ok propertyGroup ->
        ensureElement propertyGroup "AnalysisMode" "All"
        ensureElement propertyGroup "EnforceCodeStyleInBuild" "true"
        ensureElement propertyGroup "CodeAnalysisTreatWarningsAsErrors" "true"

        // Without this, you get "CSC : warning EnableGenerateDocumentationFile:
        //   Set MSBuild property 'GenerateDocumentationFile' to 'true' in
        //   project file to enable IDE0005..."
        // With it, you have to disable more XML doc checks. Dang.
        ensureElement propertyGroup "GenerateDocumentationFile" "true"

        saveCsProj csProjFile doc
        printfn $"Updated: {csProjFile}"


let runDotnetAddPackage (projectPath: string) (packageName: string) =
    let psi = ProcessStartInfo()
    psi.FileName <- "dotnet"
    psi.Arguments <- $"add \"{projectPath}\" package {packageName}"
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false

    use proc = new Process()
    proc.StartInfo <- psi
    proc.Start() |> ignore
    let output = proc.StandardOutput.ReadToEnd()
    let error = proc.StandardError.ReadToEnd()
    proc.WaitForExit()

    if proc.ExitCode = 0 then
        printfn $"✓ Added package {packageName} to {projectPath}"
    else
        printfn $"✗ Failed to add {packageName} to {projectPath}\n{error}"


let run (cmd:string) =
    let psi = ProcessStartInfo()
    psi.FileName <- Array.head (cmd.Split ' ')
    psi.Arguments <- String.Join(' ', Array.tail (cmd.Split ' '))
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false

    use proc = new Process()
    proc.StartInfo <- psi
    proc.Start() |> ignore
    let output = proc.StandardOutput.ReadToEnd()
    let error = proc.StandardError.ReadToEnd()
    proc.WaitForExit()

    if proc.ExitCode = 0 then
        Ok output
    else
        Error error


let ensureEditorConfigSettings (filePath: string) =
    // assumes there's no existing *.cs section or any matching settings
    let newSettingLines =
        [ ""
          "[*.cs]"
          "csharp_style_namespace_declarations = file_scoped            # less indentation = easier reading"
          "csharp_using_directive_placement = inside_namespace          # see https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1200.md"
          "dotnet_style_prefer_conditional_expression_over_return = false   # ... nah"
          "dotnet_analyzer_diagnostic.category-Style.severity = error   # warnings = error, otherwise you won't fix it"
          "dotnet_diagnostic.IDE0008.severity = none                    # prefer var over type names. Who doesn't like type inference?"
          "dotnet_diagnostic.IDE0130.severity = suggestion              # fixes dotnet format crash: https://github.com/dotnet/format/issues/1623"
          "dotnet_diagnostic.CA2007.severity = none                     # avoid ConfigureAwait everywhere (intended for libraries)"
          "dotnet_diagnostic.CA5394.severity = none                     # insecure RNG is fine"
          "dotnet_diagnostic.CS1591.severity = none                     # ignore missing XML docs"
          "dotnet_diagnostic.VSTHRD100.severity = error                 # never use async void"
          ]

    File.AppendAllLines(filePath, newSettingLines)


let installCsharpier projRoot =
    let manifest = Path.Combine [|projRoot; ".config/dotnet-tools.json"|]
    let doInstall = fun () ->
        match run $"dotnet tool install csharpier --tool-manifest {manifest}" with
        | Ok x -> Ok x
        | Error e -> Error $"Failed to install csharpier: {e}"
    match Path.Exists manifest with
    | true -> doInstall()
    | false ->
        match run $"dotnet new tool-manifest --project {projRoot}" with
        | Error e -> Error $"failed to install tool manifest: {e}"
        | Ok _ -> doInstall()


let ensureRunLinterBashScript projRoot =
    let path = Path.Combine [|projRoot; "lint_and_format.sh"|];
    if Path.Exists path then
        Ok "nothing to do"
    else
        File.WriteAllLines(path, [
            "#!/bin/bash"
            "CMD=$1"
            ""
            """if [[ $CMD = "check" ]]; then"""
            "    dotnet format --verify-no-changes"
            "    dotnet csharpier check ."
            """elif [[ $CMD = "fix" ]]; then"""
            "    # two formats - some fixes expose other violations"
            "    dotnet format"
            "    dotnet format"
            "    dotnet csharpier format ."
            "    echo ''"
            "    echo 'Done. Note that some manual fixes may be required.'"
            "else"
            """    echo usage: "$0 [check|fix]" """
            "fi"
        ])
        run $"chmod +x {path}"


let main (argv: string array) =
    if argv.Length < 1 then
        printfn $"Usage: dotnet fsi {__SOURCE_FILE__} <directory> [--edconfig] [--csproj] [--csharpier]"
        1
    else
        let rootDir = argv.[0]
        let hasEdConfigOption = Array.contains "--edconfig" argv
        let hasCsProjOption = Array.contains "--csproj" argv
        let hasCsharpierOption = Array.contains "--csharpier" argv
        let noOptions = not hasEdConfigOption && not hasCsProjOption && not hasCsharpierOption
        let doEdConfig = hasEdConfigOption || noOptions
        let doCsProj = hasCsProjOption || noOptions
        let doCsharpier = hasCsharpierOption || noOptions

        if doEdConfig then
            ensureEditorConfigSettings (Path.Combine [| rootDir; ".editorconfig" |])
        if doCsharpier then
            match installCsharpier rootDir with
            | Ok _ -> ()
            | Error e -> failwith e
        if doCsProj then
            let csProjFiles = findAllCsprojFiles rootDir
            for file in csProjFiles do
                addLintSettings file
                runDotnetAddPackage file "Microsoft.VisualStudio.Threading.Analyzers"
        match ensureRunLinterBashScript rootDir with
        | Ok _ -> ()
        | Error e -> failwith e

        printfn ""
        printfn "Done."
        printfn ""
        printfn "To run checks without formatting files, run:"
        printfn ""
        printfn "   ./lint_and_format.sh check"
        printfn ""
        printfn "To make all possible fixes, run:"
        printfn ""
        printfn "   ./lint_and_format.sh fix"

        0


let args = fsi.CommandLineArgs |> Array.tail
main args
