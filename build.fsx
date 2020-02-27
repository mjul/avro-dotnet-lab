#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.initEnvironment()

module Config =
    let srcDir = "src"
    let csharpSrcDir = srcDir </> "csharp"
    let fsharpSrcDir = srcDir </> "fsharp"
    let idlDir = srcDir </> "idl"
    let codeGenOutputDir = csharpSrcDir </> "GeneratedSchemaTypes" </> "generated"
    // Maps from the Avro namespaces (keys) to the corresponding generated .NET namespaces (values)
    let codeGenNamespaceMappings =
        [] |> Map.ofList

    let testDir = "tests"

Target.create "Clean" (fun _ ->
    !!(Config.srcDir @@ "/**/bin") ++ (Config.srcDir @@ "/**/obj") ++ (Config.testDir @@ "/**/bin")
    ++ (Config.testDir @@ "/**/obj") ++ Config.codeGenOutputDir |> Shell.cleanDirs)

/// Run the 'avrogen' code generator
let avrogenSchema schema outdir namespaceMappings =
    // Example command line:
    //    dotnet avrogen -s .\src\idl\transfers.avsc .\src\csharp\GeneratedSchemaTypes\generated --namespace AvroDotNetLab:AvroDotNetLab.GeneratedSchemaTypes
    let argList =
        [ yield! [ "-s"; schema; outdir ]
          for avroNamespace, dotnetNamespace in (namespaceMappings |> Map.toSeq) do
              yield! [ "--namespace"
                       (sprintf "%s:%s" avroNamespace dotnetNamespace) ] ]

    let args =
        argList
        |> Arguments.OfArgs
        |> Arguments.toWindowsCommandLine

    DotNet.exec id "avrogen" args

Target.create "CodeGenSchemas" (fun _ ->
    !!(Config.idlDir @@ "*.avsc")
    |> Seq.iter (fun schemaFile ->
        avrogenSchema schemaFile Config.codeGenOutputDir Config.codeGenNamespaceMappings |> ignore))

Target.create "CodeGen" (fun _ -> Trace.trace "Running code generation...")

Target.create "Build" (fun _ -> !!"src/**/*.*proj" |> Seq.iter (DotNet.build id))

Target.create "Test" (fun _ ->
    !!(Config.testDir @@ "**/*.*sproj")
    |> Seq.iter (DotNet.test id)
)

Target.create "All" ignore

"CodeGenSchemas" ==> "CodeGen"

"Clean" ==> "CodeGen"==> "Build" ==> "Test" ==> "All"

Target.runOrDefault "All"
