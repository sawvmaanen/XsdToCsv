using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using Autofac;

namespace XsdToCsv
{
    internal class Program
    {
        // XsdToCsv AfmeldenWerkorder.xsd AfmeldenWerkorderRequestType AfmeldenWerkorderRequest
        private static void Main(string[] args)
        {
            if (!ValidateUsage(args))
                return;

            var xsdPath = args[0];
            var xmlObjectName = args[1];
            var outputElement = args[2];
            var errorMessage = "";

            try
            {
                var container = Bootstrapper.Configure();
                var loader = container.Resolve<IXsdLoader>();
                var schemas = loader.Load(xsdPath);

                if (loader.GetValidationResult().Count(x => x.Key == XmlSeverityType.Error) > 0)
                    return;

                var transformer = container.Resolve<IXsdTransformer>();
                var result = transformer.TransformSchema(schemas, xmlObjectName, outputElement);
                WriteCsv(outputElement + ".csv", result);
            }
            catch (XsdFileNotFoundException)
            {
                errorMessage = "Xsd not found.";
            }
            catch (XsdSchemaOrElementNotFoundException)
            {
                errorMessage = "Schema or element not found.";
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
            }

            Console.WriteLine(errorMessage);

            if (Debugger.IsAttached)
                Console.ReadKey();
        }

        private static bool ValidateUsage(string[] args)
        {
            if (args != null && args.Count() == 3)
                return true;

            Console.WriteLine("Usage: XsdToCsv <XsdPath> <SchemaType> <ElementName>");
            return false;
        }

        private static void WriteCsv(string path, string csv)
        {
            using (var writer = File.CreateText(path))
                writer.Write(csv);

            Console.WriteLine(path + " created.");
        }
    }
}