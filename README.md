# Using Apache Avro on .NET

The Apache Avro serialization format is quite useful (_e.g._ for Kafka Streams).

However, the .NET documentation is lacking.

This repository contains an example of how to use it in C# and F#.

## Compiling

The code uses the (Fake)[https://fake.build] command-line tool to build:

The tools are described in the `.config\dotnet-tools.json` manifest, 
so you only have to and issue the installation and build command:

    dotnet tool restore
    dotnet fake build

The build itself is described in the (`build.fsx`)[build.fsx] file.

