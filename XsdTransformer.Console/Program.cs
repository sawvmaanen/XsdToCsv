using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using Autofac;
using XsdTransformer.Core;

namespace XsdTransformer.Console
{
    internal class Program
    {
        // XsdToCsv AfmeldenWerkorder.xsd AfmeldenWerkorderRequest.xml
        private static void Main(string[] args)
        {
            if (!ValidateUsage(args))
                return;

            var xsdPath = args[0];
            var xmlPath = args[1];
            var errorMessage = "";

            try
            {
                var container = Bootstrapper.Configure();
                var loader = container.Resolve<IXsdLoader>();
                var document = loader.Load(xsdPath, xmlPath);

                if (loader.GetValidationResult().Count(x => x.Key == XmlSeverityType.Error) > 0)
                    return;

                var transformer = container.Resolve<IXsdTransformer>();
                var result = transformer.TransformXml(document);
                WriteCsv(document.Root, result);
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

            System.Console.WriteLine(errorMessage);

            if (Debugger.IsAttached)
                System.Console.ReadKey();
        }

        private static bool ValidateUsage(string[] args)
        {
            if (args != null && args.Count() == 2)
                return true;

            System.Console.WriteLine("Usage: XsdToCsv <XsdPath> <XmlPath>");
            return false;
        }

        private static void WriteCsv(XElement element, string csv)
        {
            var path = element.Name.LocalName + ".csv";

            using (var writer = File.CreateText(path))
                writer.Write(csv);

            System.Console.WriteLine(path + " created.");
        }
    }
}