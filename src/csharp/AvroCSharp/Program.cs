using System;
using System.Collections.Generic;
using System.IO;
using Avro;
using Avro.File;
using Avro.Generic;
using Avro.Specific;

// Use our generated types:
using AvroDotNetLab.Schemas;

namespace AvroCSharp
{
    class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  AvroCSharp deserialize-datum FILE");
            Console.WriteLine("  AvroCSharp serialize-datum FILE");
            Console.WriteLine("  AvroCSharp save-file FILE");
            Console.WriteLine("  AvroCSharp load-file FILE");
        }

        private static TransferRequest CreateTransferRequest()
        {
            var result = new TransferRequest
            {
                metadata = new Metadata()
                {
                    sender = "avro-csharp",
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


        /// <summary>
        /// This will serialize a datum (a single record).
        /// It does not create an Avro file container with the schema
        /// in the beginning of the file.
        /// </summary>
        private static void SerializeSingleDatumTo(string file, TransferRequest instance)
        {
            using (var stream = new FileStream(file, FileMode.Create))
            {
                var encoder = new Avro.IO.BinaryEncoder(stream);
                // The Avro.Specific namespace is for writing a single datum (no container)
                // There is the SpecificDatumWriter for writing subtypes of ISpecificRecord 
                // (for example, classes from the codegen).
                // SpecificDefaultWriter can write anything.
                var writer = new Avro.Specific.SpecificDefaultWriter(instance.Schema);
                writer.Write(instance, encoder);
            }
        }

        /// <summary>
        /// This reads a single serialized datum from a file (not an Avro file container
        /// complete with schema, but the datum only).
        /// </summary>
        private static TransferRequest DeserializeSingleDatumFrom(string file)
        {
            var result = new TransferRequest();
            using (var stream = new FileStream(file, FileMode.Open))
            {
                var decoder = new Avro.IO.BinaryDecoder(stream);
                var reader = new Avro.Specific.SpecificDefaultReader(result.Schema, result.Schema);
                reader.Read(result, decoder);
            }

            return result;
        }

        /// <summary>
        /// Write datum to an Avro file container (an "Avro file").
        /// This is a self-contained file which includes the schema.
        /// </summary>
        private static void SerializeToAvroFileContainer(string file, TransferRequest value)
        {
            using (var stream = new FileStream(file, FileMode.Create))
            {
                // The Avro.Specific namespace is for writing a single datum (no container)
                // SpecificDatumWriter is for writing the generated classes
                var datumWriter = new SpecificDatumWriter<TransferRequest>(value.Schema);

                // Note that the Avro.File namespace is for the file containers
                // Optionally we could also specify the codec (compression and or checksums)
                using (var avroFileWriter = Avro.File.DataFileWriter<TransferRequest>.OpenWriter(datumWriter, stream))
                {
                    avroFileWriter.Append(value);
                }
            }
        }

        /// <summary>
        /// Read datum from an Avro file container (an "Avro file").
        /// This is a self-contained file which includes the schema.
        /// </summary>
        private static TransferRequest DeserializeFromAvroFileContainer(string file)
        {
            TransferRequest result = null;
            using (var avroFileReader =
                Avro.File.DataFileReader<TransferRequest>.OpenReader(file))
            {
                // Just read a single record
                result = avroFileReader.Next();
                // If we want to read all data in the file we can stream
                // all rows using avroFileReader.NextEntries;
            }
            return result;
        }


        private static string FormatAccount(object account)
        {
            var result = "";
            if (account is DanishAccount)
            {
                var dkAccount = (DanishAccount) account;
                result = $"Danish account {dkAccount.regnr}-{dkAccount.kontonr}";
            }
            else
            {
                var ibanAccount = (IbanAccount) account;
                result = $"IBAN account {ibanAccount.countryCode}{ibanAccount.checkDigits} {ibanAccount.BBAN}";
            }

            return result;
        }


        private static void PrintTransferRequest(TransferRequest value)
        {
            Console.WriteLine(
                $"Transfer amount {LogicalTypesHack.FromLogicalTypeMoneyDecimal(value.amount.amount)} {value.amount.currencyCode} from {FormatAccount(value.from)} to {FormatAccount(value.to)} on date {LogicalTypesHack.FromLogicalTypeDate(value.date):yyyy-MM-dd}");
        }

        public static void Main(string[] args)
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
                    case "serialize-datum":
                        Console.WriteLine("Serializing transfer request");
                        var valueToSerialize = CreateTransferRequest();
                        PrintTransferRequest(valueToSerialize);
                        SerializeSingleDatumTo(file, valueToSerialize);
                        break;
                    case "deserialize-datum":
                        Console.WriteLine("Deserializing transfer request");
                        var deserializedValue = DeserializeSingleDatumFrom(file);
                        PrintTransferRequest(deserializedValue);
                        break;
                    case "save-file":
                        Console.WriteLine($"Saving transfer request to Avro file container {file}");
                        var transferRequestToSave = CreateTransferRequest();
                        PrintTransferRequest(transferRequestToSave);
                        SerializeToAvroFileContainer(file, transferRequestToSave);
                        break;
                    case "load-file":
                        Console.WriteLine($"Loading transfer request from Avro file container {file}");
                        var deserializedTransferRequest = DeserializeFromAvroFileContainer(file);
                        PrintTransferRequest(deserializedTransferRequest);
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }
        }
    }
}