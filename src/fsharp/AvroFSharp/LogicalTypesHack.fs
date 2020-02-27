/// The Avro library (Apache.Avro version 1.9.1) does not implement the
/// "logical types" in the Avro specification (dates, timestamps, fixed-precision
/// number such as money...).
/// This module is a hack to work around that.
namespace AvroDotNetLab.AvroFSharp

module LogicalTypesHack =

    open System
    open System.Numerics

    // Hack: to keep it simple, we always use two decimals.
    let MoneyScale = 2
    let MoneyScaleMultiplier = 10. ** (double MoneyScale) |> decimal

    /// Read the logical type Date from its Avro representation as an integer.
    let fromLogicalTypeDateInt (value: int32) =
        // A quick hack ad hoc conversion based on https://github.com/timjroberts/avro/blob/AVRO-2359-logical-types/lang/csharp/src/apache/main/Util/Date.cs
        // while we are waiting for the PR to merge support for logical types in the C# libray.
        // Dates are encoded as an int representing the days since the Unix Epoch.
        let daysSinceEpoch = float value
        DateTime.UnixEpoch.AddDays(daysSinceEpoch).Date

    /// Convert a DateTime to its logical type Date int representation.
    let toLogicalTypeDateInt (value: DateTime) =
        let daysSinceEpoch = Convert.ToInt32(value.Subtract(DateTime.UnixEpoch).TotalDays)
        daysSinceEpoch

    /// Convert a decimal to its logical type decimal (byte array) representation.
    let toLogicalTypeMoneyDecimalBytes (value: decimal) =
        // hack: we use a fixed scale
        let unscaled = BigInteger(Decimal.Round(MoneyScaleMultiplier * value, 0))
        // From the spec:
        // The byte array must contain the two's-complement representation of the
        // unscaled integer value in big-endian byte order.
        unscaled.ToByteArray(isUnsigned = false, isBigEndian = true)

    
    /// Convert the byte array representing a logical type decimal to its Decimal representation.
    let fromLogicalTypeMoneyDecimalBytes (value: byte array) : decimal =
        let unscaled = BigInteger(ReadOnlySpan(value), isUnsigned = false, isBigEndian = true)
        (decimal unscaled) / MoneyScaleMultiplier 
