open System.IO
open System.Text
open System.Xml
open System.Xml.Linq
open System
open System.Diagnostics


let getAllCsprojFiles (directory: string) =
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

        // without this, you get "CSC : warning EnableGenerateDocumentationFile: Set MSBuild property 'GenerateDocumentationFile' to 'true' in project file to enable IDE0005..."
        // with it, you have to disable more XML doc checks. Dang.
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
          "dotnet_diagnostic.CA2007.severity = none                     # avoid ConfigureAwait everywhere (intended for libraries)"
          "dotnet_diagnostic.CS1591.severity = none                     # ignore missing XML docs"
          "dotnet_diagnostic.CA5394.severity = none                     # insecure RNG is fine"
          "dotnet_diagnostic.VSTHRD100.severity = error                 # never use async void"
          "dotnet_diagnostic.SA0001.severity = none                     # ignore XML comment analysis"
          "dotnet_diagnostic.SA1010.severity = none                     # square bracket spacing. conflicts with SA1001"
          "dotnet_diagnostic.SA1101.severity = none                     # 'this.' doesn't work when doing record { with SomeField = ... }"
          "dotnet_diagnostic.SA1118.severity = none                     # enable multiline params. Just my pref. Using variables is just as bad IMO."
          "dotnet_diagnostic.SA1600.severity = none                     # ignore missing XML docs"
          "dotnet_diagnostic.SA1633.severity = none                     # source files don't need 'header' doc comments" ]

    File.AppendAllLines(filePath, newSettingLines)


let main (argv: string array) =
    if argv.Length <> 1 then
        printfn $"Usage: dotnet fsi {__SOURCE_FILE__} <directory>"
        1
    else
        let rootDir = argv.[0]
        ensureEditorConfigSettings (Path.Combine [| rootDir; ".editorconfig" |])
        let files = getAllCsprojFiles rootDir

        for file in files do
            addLintSettings file
            runDotnetAddPackage file "Microsoft.VisualStudio.Threading.Analyzers"
            runDotnetAddPackage file "StyleCop.Analyzers"

        0


let args = fsi.CommandLineArgs |> Array.tail
main args
