using System;
using System.IO;
using Avro;
using Avro.Specific;

// Use our generated types:
using AvroDotNetLab;

namespace AvroCSharp
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  AvroCSharp deserialize file");
            Console.WriteLine("  AvroCSharp serialize file");
        }

        static TransferRequest CreateTransferRequest()
        {
            var result = new TransferRequest
            {
                metadata = new Metadata()
                {
                    sender = "upstream-system",
                    senderCorrelationId = "tx-request-1234",
                },
                amount = new Amount
                {
                    amount = LogicalTypesHack.ToLogicalTypeMoneyDecimal(100),
                    currencyCode = "DKK"
                },
                date = LogicalTypesHack.ToLogicalTypeDate(new DateTime(2020, 02, 05)),
                from = new DanishAccount()
                {
                    regnr = "1000",
                    kontonr = "0000001234",
                },
                to = new IbanAccount()
                {
                    countryCode = "DK",
                    checkDigits = 12,
                    BBAN = "9999000999",
                },
                senderIdentifier = "Transfer ref. tx-request-1234",
                recipientIdentifier = "Transfer ref. payment 1234"
            };
            return result;
        }


        static void SerializeTo(string file, TransferRequest instance)
        {
            using (var stream = new FileStream(file, FileMode.Create))
            {
                var encoder = new Avro.IO.BinaryEncoder(stream);
                var writer = new Avro.Specific.SpecificDefaultWriter(instance.Schema);
                writer.Write(instance, encoder);
            }
        }

        static TransferRequest DeserializeFrom(string file)
        {
            TransferRequest result = new TransferRequest();
            using (var stream = new FileStream(file, FileMode.Open))
            {
                var decoder = new Avro.IO.BinaryDecoder(stream);
                var reader = new Avro.Specific.SpecificDefaultReader(result.Schema, result.Schema);
                reader.Read(result, decoder);
            }
            return result;
        }

        static string FormatAccount(object account)
        {
            var result = "";
            if (account is DanishAccount)
            {
                var dkAccount = (DanishAccount)account;
                result = $"Danish account {dkAccount.regnr}-{dkAccount.kontonr}";
            }
            else
            {
                var ibanAccount = (IbanAccount)account;
                result = $"IBAN account {ibanAccount.countryCode}{ibanAccount.checkDigits} {ibanAccount.BBAN}";
            }
            return result;
        }



        static void PrintTransferRequest(TransferRequest value)
        {
            Console.WriteLine($"Transfer amount {LogicalTypesHack.FromLogicalTypeMoneyDecimal(value.amount.amount)} {value.amount.currencyCode} from {FormatAccount(value.from)} to {FormatAccount(value.to)} on date {LogicalTypesHack.FromLogicalTypeDate(value.date):yyyy-MM-dd}");
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
            }
            else
            {
                var action = args[0];
                var file = args[1];
                switch (action)
                {
                    case "serialize":
                        Console.WriteLine("Serializing transfer request");
                        var value = CreateTransferRequest();
                        PrintTransferRequest(value);
                        SerializeTo(file, value);
                        break;
                    case "deserialize":
                        Console.WriteLine("Deserializing transfer request");
                        var deserialized = DeserializeFrom(file);
                        PrintTransferRequest(deserialized);
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }
        }
    }
}
