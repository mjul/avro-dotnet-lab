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

Target.initEnvironment ()

module Config = 
  let srcDir = "src"
  let csharpSrcDir = srcDir </> "csharp"
  let fsharpSrcDir = srcDir </> "fsharp"
  let idlDir = srcDir </> "idl"
  let codeGenOutputDir = csharpSrcDir </> "GeneratedSchemaTypes" </> "generated"

Target.create "Clean" (fun _ ->
    !! (Config.srcDir @@ "/**/bin")
    ++ (Config.srcDir @@ "/**/obj")
    ++ Config.codeGenOutputDir
    |> Shell.cleanDirs 
)

/// Run the 'avrogen' code generator
let avrogenSchema schema outdir =
  // Example command line:
  //    dotnet avrogen -s .\src\idl\transfers.avsc .\src\csharp\GeneratedSchemaTypes\generated
  let args = [ "-s"; schema; outdir ] |> Arguments.OfArgs |> Arguments.toWindowsCommandLine
  DotNet.exec id "avrogen" args

Target.create "CodeGenSchemas" (fun _ ->
  avrogenSchema (Config.idlDir </> "transfers.avsc") Config.codeGenOutputDir |> ignore
)

Target.create "CodeGen" (fun _ ->
  Trace.trace "Running code generation..."
)



Target.create "Build" (fun _ ->
    !! "src/**/*.*proj"
    |> Seq.iter (DotNet.build id)
)

Target.create "All" ignore

"Clean"
  ==> "Build"
  ==> "All"

Target.runOrDefault "All"
