module Tests

open System
open Xunit

[<Fact>]
let ``Logical type decimal roundtrip Csharp to Fsharp`` () =
    let value = 163.84m
    let serializedWithCSharp = AvroCSharp.LogicalTypesHack.ToLogicalTypeMoneyDecimal value
    let deserializedWithFSharp = AvroDotNetLab.AvroFSharp.LogicalTypesHack.fromLogicalTypeMoneyDecimalBytes serializedWithCSharp
    Assert.Equal(value, deserializedWithFSharp)

[<Fact>]
let ``Logical type decimal roundtrip Fsharp to Csharp`` () =
    let value = 163.84m
    let serializedWithFSharp = AvroDotNetLab.AvroFSharp.LogicalTypesHack.toLogicalTypeMoneyDecimalBytes value
    let deserializedWithCSharp = AvroCSharp.LogicalTypesHack.FromLogicalTypeMoneyDecimal serializedWithFSharp
    Assert.Equal(value, deserializedWithCSharp)
