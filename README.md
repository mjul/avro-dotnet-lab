# Using Apache Avro on .NET

The [Apache Avro](https://avro.apache.org/) serialization format is quite useful (_e.g._ for Kafka Streams) or 
when you need to have a schema for your data exchange.

You will face three problems using it on .NET, however:
    1) The .NET documentation is quite sparse.
    2) There are multiple implementations, so you will need to pick the right one.
    3) None of the implementations are complete and support the full Avro spec.

This repository contains an example of how to work with Avro in C# and F#.

- The Avro schemas are defined in the [`src\idl`](src/idl) folder.
- Classes for schema types are generated with `avrogen`, see the [GeneratedSchemaTypes](src/csharp/GeneratedSchemaTypes) project.
- The F# tool to read and write records is in [src\fsharp\AvroFSharp](src/fsharp/AvroFSharp)
- The C# tool is in [src\csharp\AvroCSharp](src/csharp/AvroCSharp)

## Building the Code

The code uses the [Fake](https://fake.build) command-line tool to build:

The tools are described in the `.config\dotnet-tools.json` manifest, 
so you only have to and issue the installation and build command:

    dotnet tool restore
    dotnet fake build

The build itself is described in the [`build.fsx`](build.fsx) file.


## Defining Schemas

Avro offers a succinct IDL format and a verbose JSON format to describe 
the schemas and protocols.

The definitions are in the [`src\idl`](src/idl) folder.

### Logical Types
Note that there is no support for converting .NET types to and from `logicalTypes` yet (_e.g._ to date or fixed decimal) in the Apache Avro library nor in the Confluent fork.
It is supported in _e.g._ the Java library. It is underway in PR [AVRO-2359: Support Logical Types in C#](https://github.com/apache/avro/pull/492).


## Code Generation
I tried different code generators.

At the time of writing (February 2020) the Confluent fork is the most mature,
but it has been incorporated into the official Apache tooling and
will be discontinued when it is released with the Avro 1.10 release (soon?).

My advice is to use `Apache.Avro` and the code-generator `Apache.Avro.Tools` unless
there is something in the Confluent fork you absolutely need for the moment.

### Chr.Avro Code Generator

The [Chr.Avro](https://engineering.chrobinson.com/dotnet-avro/) code generator looked promising, but
could not handle union types (_e.g._ the two types of bank accounts in the example IDL):

    dotnet tool install Chr.Avro.Cli --global

Then, 

    Get-Content .\idl\transfers.avsc | dotnet avro generate | Out-File .\src\csharp\GeneratedSchemaTypes\generated\Transfers.cs

Conclusion: *Not recommended.*

### Confluent Code Generator

The [Confluent Code Generator](https://www.nuget.org/packages/Confluent.Apache.Avro.AvroGen/) does a better job.
Unions are represented as the .NET type `object` but that will do.

You can install it as a global tool like this:

    dotnet tool install -g Confluent.Apache.Avro.AvroGen

Then to generate the code:

    avrogen -s .\idl\transfers.avsc  .\src\csharp\GeneratedSchemaTypes\generated\

Note that it is being deprecated since Apache has started updating their tool again.

### Apache Avro Tools Code Generator

In the end this is what we are using. After some years of not being good enough, it has now caught up with the Confluent fork and
once 1.10 is release (in 2020?) the Confluent fork will be discontinued.

To install into the `dotnet-tools.json` manifest (already done):

    dotnet tool install Apache.Avro.Tools --version 1.9.1

Then to install the local tools:

    dotnet tool restore

And to generate the code into the `generated` directory:

    dotnet avrogen .\idl\transfers.avsc  .\src\csharp\GeneratedSchemaTypes\generated\



## Developer Information

The C# project is a skeleton project file used to build the generated code when the Avro schemas have been compiled.

### Visual Studio Code

Configure VS code to use [dotnet fsi](https://github.com/ionide/ionide-vscode-fsharp/issues/1237).

