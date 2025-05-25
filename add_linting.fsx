open System
open System.IO
open System.Xml.Linq

let getAllCsprojFiles (directory: string) =
    Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories)

let ensureElement (parent: XElement) (name: string) (value: string) =
    match parent.Element(XName.Get name) with
    | null ->
        let added = XElement(XName.Get name, value)
        parent.Add(added)
        added
    | elem ->
        elem.Value <- value
        elem

let updateCsprojFile (filePath: string) =
    try
        let doc = XDocument.Load(filePath)
        let ns = doc.Root.Name.Namespace

        let propertyGroup = ensureElement doc.Root "PropertyGroup" ""

        ensureElement propertyGroup "AnalysisMode" "All" |> ignore
        ensureElement propertyGroup "CodeAnalysisTreatWarningsAsErrors" "true" |> ignore

        doc.Save(filePath)
        printfn $"Updated: {filePath}"
    with ex ->
        printfn $"Error processing {filePath}: {ex.Message}"

let main (argv: string array) =
    if argv.Length <> 1 then
        printfn $"Usage: dotnet fsi {__SOURCE_FILE__} <directory>"
        1
    else
        let rootDir = argv.[0]
        let files = getAllCsprojFiles rootDir
        for file in files do
            updateCsprojFile file
        0


let args = fsi.CommandLineArgs |> Array.tail
main args
