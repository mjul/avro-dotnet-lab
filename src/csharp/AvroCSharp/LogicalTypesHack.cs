using System;
using System.Numerics;

namespace AvroCSharp
{
    public class LogicalTypesHack
    {
        private const int MoneyScale = 2;
        private static readonly decimal MoneyScaleMultiplier = Convert.ToDecimal(Math.Pow(10.0, MoneyScale));
            
        internal static DateTime FromLogicalTypeDate(Int32 value)
        {
            // A quick hack ad hoc conversion based on https://github.com/timjroberts/avro/blob/AVRO-2359-logical-types/lang/csharp/src/apache/main/Util/Date.cs
            // while we are waiting for the PR to merge support for logical types in the C# libray.
            // Dates are encoded as an int representing the days since the Unix Epoch.
            var daysSinceEpoch = Convert.ToDouble(value);
            return DateTime.UnixEpoch.AddDays(daysSinceEpoch).Date;
        }
        internal static Int32 ToLogicalTypeDate(DateTime value)
        {
            var daysSinceEpoch = Convert.ToInt32(value.Subtract(DateTime.UnixEpoch).TotalDays);
            return daysSinceEpoch;
        }


        public static byte[] ToLogicalTypeMoneyDecimal(decimal value)
        {
            // hack: we use a fixed scale of two decimals
            var unscaled = new BigInteger(MoneyScaleMultiplier * value);
            return unscaled.ToByteArray(isUnsigned: false, isBigEndian: true);
        }

        public static Decimal FromLogicalTypeMoneyDecimal(byte[] value)
        {
            var unscaled = new BigInteger(new ReadOnlySpan<byte>(value), isUnsigned:false, isBigEndian:true);
            return ((decimal)unscaled)/MoneyScaleMultiplier; 
        }
    }
}