module Tests

open AvroDotNetLab.AvroFSharp
open System
open Xunit

type LogicalTypesHackTests () = 

    [<Theory>]
    [<InlineData(1969,12,31)>] // before epoch
    [<InlineData(1970,1,1)>] // epoch
    [<InlineData(2020,2,29)>] // leap day
    let ``Logical date serialization is bijective`` (year:int) (month:int) (day:int) =
        let date = DateTime(year,month,day).Date
        let actual = date |> LogicalTypesHack.toLogicalTypeDateInt |> LogicalTypesHack.fromLogicalTypeDateInt
        Assert.Equal(date, actual)

    [<Theory>]
    [<InlineData(1969,12,31,-1)>]
    [<InlineData(1970,1,1,0)>]
    [<InlineData(2020,2,29,18321)>]
    let ``Logical date serialization is to days since Unix Epoch`` (year:int) (month:int) (day:int) (expectedSerializedValue:int) =
        let date = DateTime(year,month,day).Date
        let actual = date |> LogicalTypesHack.toLogicalTypeDateInt
        Assert.Equal(expectedSerializedValue, actual)