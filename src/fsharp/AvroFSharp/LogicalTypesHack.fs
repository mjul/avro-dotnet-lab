namespace AvroDotNetLab.AvroFSharp

/// The Avro library (Apache.Avro version 1.9.1) does not implement the
/// "logical types" in the Avro specification (dates, timestamps, fixed-precision 
/// number such as money...).
/// This module is a hack to work around that.
module LogicalTypesHack = 

    open System
    open System.IO
    open Avro
    open Avro.IO
    open Avro.Generic

    let MoneyScale = 100m
    let IntSchema = PrimitiveSchema.NewInstance("int", null)

    /// Quick hack to write an int to its Avro stream representation in a byte array.
    let toAvroIntBytes value =
        let writer = GenericWriter<int>(IntSchema)
        use ms = new MemoryStream() 
        let encoder = BinaryEncoder(ms)
        writer.Write(value, encoder)
        let result = ms.ToArray()
        result

    /// Quick hack to read an int from its Avro stream representation in the byte array.
    let fromAvroIntBytes (value:byte array) = 
        let reader = GenericReader<int>(IntSchema, IntSchema)
        use ms = new MemoryStream(value)
        let decoder = BinaryDecoder(ms)
        reader.Read(0, decoder)


    /// Read the logical type Date from its Avro representation as a byte array.
    let fromLogicalTypeDateBytes (value: byte array) =
        // A quick hack ad hoc conversion based on https://github.com/timjroberts/avro/blob/AVRO-2359-logical-types/lang/csharp/src/apache/main/Util/Date.cs
        // while we are waiting for the PR to merge support for logical types in the C# libray.
        // Dates are encoded as an int representing the days since the Unix Epoch.
        let daysSinceEpoch = fromAvroIntBytes value
        DateTime.UnixEpoch.AddDays(float daysSinceEpoch).Date
    
    let toLogicalTypeDateBytes (value:DateTime) =
        let daysSinceEpoch = Convert.ToInt32(value.Subtract(DateTime.UnixEpoch).TotalDays)
        toAvroIntBytes daysSinceEpoch
    

    let toLogicalTypeMoneyDecimalBytes (value:decimal) =
        // hack: we use a fixed scale of two decimals
        let unscaled = Convert.ToInt32(MoneyScale * value);
        toAvroIntBytes unscaled

    let fromLogicalTypeMoneyDecimalBytes (value: byte array) =
        let unscaled = fromAvroIntBytes(value)
        ((decimal)unscaled)/MoneyScale 