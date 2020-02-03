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

    /// Serialize a TransferRequest to a file
    let serializeTo file (value:ISpecificRecord) = 
        use stream = new FileStream(file, FileMode.Create)
        let encoder = BinaryEncoder(stream)
        let writer = SpecificDefaultWriter(value.Schema)
        writer.Write(value, encoder)
        ignore()

    let createTransferRequest () = 
        TransferRequest(
            metadata = Metadata(sender="avro-fsharp", senderCorrelationId="tx-fs-1"),
            amount = Amount(amount=LogicalTypesHack.toLogicalTypeMoneyDecimalBytes(200m), currencyCode="DKK"),
            date = LogicalTypesHack.toLogicalTypeDateBytes(new DateTime(2020,02,4)),
            from = DanishAccount(regnr = "2000", kontonr = "1111222333"),
            ``to`` = IbanAccount(countryCode = "DK", checkDigits = 44, BBAN="1111000333"),
            senderIdentifier = "Award payment tx-fs-1",
            recipientIdentifier = "F# award reference code tx-fs-1"
            )

    /// Deserialize a TransferRequest from a file
    let deserializeFrom file = 
        let result = TransferRequest()
        use stream = new FileStream(file, FileMode.Open)
        let decoder = BinaryDecoder(stream)
        let reader = SpecificDefaultReader(result.Schema, result.Schema)
        reader.Read(result, decoder)

    let printTransferRequest (value:TransferRequest) = 
        let formatAccount (account:Object) = 
            match account with 
            | :? DanishAccount as da -> sprintf "Danish account %s-%s" da.regnr da.kontonr
            | :? IbanAccount as iban -> sprintf "IBAN account %s%i %s" iban.countryCode iban.checkDigits iban.BBAN
            | _ -> "Unknown"
        printfn "Transfer request: on %s transfer %M %s from %s to %s" 
            ((value.date |> fromLogicalTypeDateBytes).ToString("yyyy-MM-dd"))
            (value.amount.amount |> fromLogicalTypeMoneyDecimalBytes) (value.amount.currencyCode)
            (value.from |> formatAccount)
            (value.``to`` |> formatAccount)

    [<EntryPoint>]
    let main argv =
        match argv with 
        | [|"serialize"; file|] ->
            printfn "Serializing request to %s..." file
            let request = createTransferRequest ()
            serializeTo file request
            printTransferRequest request |> ignore
            0
        | [|"deserialize"; file|] -> 
            printfn "Deserializing request from %s..." file
            deserializeFrom file |> printTransferRequest |> ignore
            0
        | _ -> 
            printUsage () |> ignore
            1