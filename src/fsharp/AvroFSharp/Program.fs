namespace AvroDotNetLab.AvroFSharp

module Program = 
    open System
    open System.IO
    open Avro
    open Avro.IO
    open Avro.Specific

    open AvroDotNetLab.AvroFSharp.LogicalTypesHack
    open AvroDotNetLab.Schemas

    let printUsage () =
        printfn "Usage:"
        printfn "  AvroFSharp serialize FILE"
        printfn "  AvroFSharp deserialize FILE"

    /// Serialize a single datum TransferRequest to a file
    /// You can use this when the schemas have already been exchanged.
    /// For file storage, you would normally write data into
    /// an Avro "File Container" which contains the schema and the data rows.
    let serializeDatumTo file (value:ISpecificRecord) = 
        use stream = new FileStream(file, FileMode.Create)
        let encoder = BinaryEncoder(stream)
        let writer = SpecificDefaultWriter(value.Schema)
        writer.Write(value, encoder)
        ignore()

    let createTransferRequest () = 
        TransferRequest(
            metadata = Metadata(sender="avro-fsharp", senderCorrelationId="tx-fs-1"),
            amount = Amount(amount=LogicalTypesHack.toLogicalTypeMoneyDecimalBytes(200m), currencyCode="DKK"),
            date = LogicalTypesHack.toLogicalTypeDateInt(new DateTime(2020,02,4)),
            from = DanishAccount(regnr = "2000", kontonr = "1111222333"),
            ``to`` = IbanAccount(countryCode = "DK", checkDigits = 44, BBAN="1111000333"),
            senderIdentifier = "Award payment tx-fs-1",
            recipientIdentifier = "F# award reference code tx-fs-1"
            )

    /// Deserialize a TransferRequest datum from a file (note: not from a File Container)
    let deserializeDatumFrom file = 
        let result = TransferRequest()
        use stream = new FileStream(file, FileMode.Open)
        let decoder = BinaryDecoder(stream)
        // The "Avro.Specific" namespace is for individual records
        let reader = SpecificDefaultReader(result.Schema, result.Schema)
        reader.Read(result, decoder)

    /// <summary>
    /// Write a record to an Avro File Container (an "Avro file").
    /// </summary>
    /// <remarks>
    /// File Containers contain both the schema and the data.
    /// </remarks>
    let serializeToFileContainer file (request:TransferRequest) =
        use stream = new FileStream(file, FileMode.Create)
        let datumWriter = Avro.Specific.SpecificDatumWriter<TransferRequest>(request.Schema)
        use fileWriter = Avro.File.DataFileWriter<TransferRequest>.OpenWriter(datumWriter, stream)
        fileWriter.Append(request)
    
    /// <summary>
    /// Read a record from an Avro File Container (an "Avro file").
    /// </summary>
    /// <remarks>
    /// File Containers contain both the schema and the data.
    /// </remarks>
    let deserializeFromFileContainer (file:string) : TransferRequest=
        let result = TransferRequest()
        use fileReader = Avro.File.DataFileReader<TransferRequest>.OpenReader(file, TransferRequest._SCHEMA)
        // fileReader.NextEntries can be used to read all data
        // Here we just read a single row (datum / record)
        fileReader.Next()

        
    let printTransferRequest (value:TransferRequest) = 
        let formatAccount (account:Object) = 
            match account with 
            | :? DanishAccount as da -> sprintf "Danish account %s-%s" da.regnr da.kontonr
            | :? IbanAccount as iban -> sprintf "IBAN account %s%i %s" iban.countryCode iban.checkDigits iban.BBAN
            | _ -> "Unknown"
        printfn "Transfer request: on %s transfer %M %s from %s to %s" 
            ((value.date |> fromLogicalTypeDateInt).ToString("yyyy-MM-dd"))
            (value.amount.amount |> fromLogicalTypeMoneyDecimalBytes) (value.amount.currencyCode)
            (value.from |> formatAccount)
            (value.``to`` |> formatAccount)

    [<EntryPoint>]
    let main argv =
        match argv with 
        | [|"serialize"; file|] ->
            printfn "Serializing request to %s..." file
            let request = createTransferRequest ()
            serializeDatumTo file request
            printTransferRequest request |> ignore
            0
        | [|"deserialize"; file|] -> 
            printfn "Deserializing request from %s..." file
            deserializeDatumFrom file |> printTransferRequest |> ignore
            0
        | [|"save-file"; file|] ->
            printfn "Saving request to Avro File Container %s..." file
            let request = createTransferRequest ()
            serializeToFileContainer file request
            printTransferRequest request |> ignore
            0
        | [|"load-file"; file|] -> 
            printfn "Loading request from Avro File Container %s..." file
            deserializeFromFileContainer file |> printTransferRequest |> ignore
            0
        | _ -> 
            printUsage () |> ignore
            1