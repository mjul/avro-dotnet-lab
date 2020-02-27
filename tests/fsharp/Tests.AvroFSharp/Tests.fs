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
        let today = DateTime(year,month,day).Date
        let actual = today |> LogicalTypesHack.toLogicalTypeDateBytes |> LogicalTypesHack.fromLogicalTypeDateBytes
        Assert.Equal(today, actual)

