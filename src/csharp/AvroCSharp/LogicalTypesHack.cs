using System;
using System.IO;
using Avro;
using Avro.Specific;

namespace AvroCSharp
{
    internal class LogicalTypesHack
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);
        const int MONEY_SCALE = 100;

        internal static byte[] ToAvroInt(int value) {
            // Quick hack to write an int to its Avro stream representation in a byte array.
            var intSchema = PrimitiveSchema.NewInstance("int", null);
            var writer = new Avro.Generic.GenericWriter<int>(intSchema);
            byte[] result = null;
            using (var ms = new MemoryStream()) {
                var encoder = new Avro.IO.BinaryEncoder(ms);
                writer.Write(value, encoder);
                result = ms.GetBuffer();
            }
            return result;
        }

        internal static int FromAvroInt(byte[] value) {
            // Quick hack to read an int from its Avro stream representation in the byte array.
            var intSchema = PrimitiveSchema.NewInstance("int", null);
            var reader = new Avro.Generic.GenericReader<int>(intSchema, intSchema);
            int result = -1;
            using (var ms = new MemoryStream(value)) {
                var decoder = new Avro.IO.BinaryDecoder(ms);
                result = reader.Read(0, decoder);
            }
            return result;
        }


        internal static DateTime FromLogicalTypeDate(byte[] value)
        {
            // A quick hack ad hoc conversion based on https://github.com/timjroberts/avro/blob/AVRO-2359-logical-types/lang/csharp/src/apache/main/Util/Date.cs
            // while we are waiting for the PR to merge support for logical types in the C# libray.
            // Dates are encoded as an int representing the days since the Unix Epoch.
            var daysSinceEpoch = FromAvroInt(value);
            return UnixEpoch.AddDays(daysSinceEpoch).Date;
        }
        internal static byte[] ToLogicalTypeDate(DateTime value)
        {
            int daysSinceEpoch = Convert.ToInt32(value.Subtract(UnixEpoch).TotalDays);
            return ToAvroInt(daysSinceEpoch);
        }

        
        internal static byte[] ToLogicalTypeMoneyDecimal(decimal value)
        {
            // hack: we use a fixed scale of two decimals
            var unscaled = Convert.ToInt32(MONEY_SCALE * value);
            return ToAvroInt(unscaled);
        }

        internal static Decimal FromLogicalTypeMoneyDecimal(byte[] value)
        {
            var unscaled = FromAvroInt(value);
            return ((decimal)unscaled)/MONEY_SCALE; 
        }
    }
}