using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace XsdToCsv
{
    internal class Program
    {
        private static int _schemaValidationErrors;
        private const string Indent = "  ";
        private static readonly string Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        private static readonly Dictionary<string, XmlSchemaComplexType> DescribedTypes = new Dictionary<string, XmlSchemaComplexType>();

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
                var schema = LoadXmlSchema(xsdPath);
                var schemaSet = new XmlSchemaSet();
                schemaSet.ValidationEventHandler += ValidationEventHandler;
                schemaSet.Add(schema);
                schemaSet.Compile();

                if (_schemaValidationErrors > 0)
                    return;

                var schemas = schemaSet.Schemas();
                var csv = GetHeaderCsv();

                csv += GetSchemaCsv(schemas, xmlObjectName, outputElement);
                WriteCsv(outputElement + ".csv", csv);
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

        private static string GetHeaderCsv()
        {
            var headers = new[] { "Elementnaam", "Multi", "Type", "Omschrijving", "Voorbeeld" };
            return headers.Aggregate("", (c, n) => c + (string.IsNullOrWhiteSpace(c) ? "" : Separator) + n) + "\n";
        }

        private static string GetSchemaCsv(ICollection schemas, string objectName, string outputElement)
        {
            var schemaType = FindSchemaType(schemas, objectName);

            if (schemaType == null)
            {
                var element = FindSchemaElement(schemas, objectName);
                if (element == null)
                    throw new XsdSchemaOrElementNotFoundException();
                return GetElementCsv(0, element);
            }
            return GetSchemaTypeCsv(0, schemaType, outputElement, 1, 1);
        }

        private static XmlSchemaComplexType FindSchemaType(IEnumerable schemas, string name)
        {
            foreach (XmlSchema s in schemas)
            {
                foreach (var t in s.SchemaTypes.Values)
                {
                    var schemaType = t as XmlSchemaComplexType;
                    if (schemaType != null && schemaType.Name == name)
                        return schemaType;
                }
            }

            return null;
        }

        private static XmlSchemaElement FindSchemaElement(IEnumerable schemas, string name)
        {
            foreach (XmlSchema s in schemas)
            {
                foreach (var e in s.Elements.Values)
                {
                    var element = e as XmlSchemaElement;
                    if (element != null && element.Name == name)
                        return element;
                }
            }

            return null;
        }

        private static string Quoted(string value, string quote = "\"")
        {
            return string.Format("{0}{1}{0}", quote, value);
        }

        private static string BuildCsvLine(int level, string name, decimal minOccurs, decimal maxOccurs, string typeName, string example = "")
        {
            var strMaxOccures = maxOccurs == decimal.MaxValue ? "*" : maxOccurs.ToString(CultureInfo.InvariantCulture);
            var strMinOccurs = minOccurs == 0 && strMaxOccures == "*" ? "*" : minOccurs.ToString(CultureInfo.InvariantCulture);
            var format = strMinOccurs != strMaxOccures ? "{0}..{1}" : "{0}";
            var occurs = string.Format(format, strMinOccurs, strMaxOccures);

            name = string.Concat(Enumerable.Repeat(Indent, level)) + name;
            return string.Format("{1}{0}{2}{0}{3}{0}{0}{4}\n", Separator, Quoted(name), Quoted(occurs), Quoted(typeName), example);
        }

        private static string GetComplexTypeCsv(int level, XmlSchemaType schemaType)
        {
            var csv = "";
            var complexType = schemaType as XmlSchemaComplexType;

            if (complexType != null && !DescribedTypes.ContainsKey(complexType.QualifiedName.ToString()))
            {
                if (!string.IsNullOrWhiteSpace(complexType.QualifiedName.ToString()))
                    DescribedTypes.Add(complexType.QualifiedName.ToString(), complexType);

                var sequence = complexType.ContentTypeParticle as XmlSchemaSequence;

                if (sequence != null)
                    foreach (var item in sequence.Items)
                    {
                        var childElement = item as XmlSchemaElement;

                        if (childElement != null)
                        {
                            var childComplexType = childElement.ElementSchemaType as XmlSchemaComplexType;

                            if (childComplexType != null)
                                csv += GetSchemaTypeCsv(level, childComplexType, childElement.QualifiedName.Name, childElement.MinOccurs, childElement.MaxOccurs);
                            else
                                csv += GetElementCsv(level, childElement);
                        }
                    }
            }
            return csv;
        }

        private static List<string> GetFacets(XmlSchemaElement element)
        {
            //(element.Annotation.Items[0] as XmlSchemaDocumentation).
            //var simpleType = element.ElementSchemaType as XmlSchemaSimpleType;
            //if (simpleType == null)
            //    return null;

            //var restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            //return restriction == null ? null : restriction.Facets.Cast<XmlSchemaFacet>().Where(x => x is XmlSchemaEnumerationFacet).Select(x => x.Value).ToList();
            return new List<string>();
        }

        private static string GetElementCsv(int level, XmlSchemaElement element)
        {
            var facets = GetFacets(element).Aggregate("", (c, n) => c + (string.IsNullOrWhiteSpace(c) ? "" : ", ") + n);
            var csv = BuildCsvLine(level, element.QualifiedName.Name, element.MinOccurs, element.MaxOccurs, element.ElementSchemaType.TypeCode.ToString(), facets);
            return csv + GetComplexTypeCsv(level + 1, element.SchemaType);
        }

        private static string GetSchemaTypeCsv(int level, XmlSchemaType schemaType, string outputElement, decimal minOccurs, decimal maxOccurs)
        {
            var csv = BuildCsvLine(level, outputElement, minOccurs, maxOccurs, schemaType.Name);
            return csv + GetComplexTypeCsv(level + 1, schemaType);
        }

        private static void ValidationEventHandler(object sender, ValidationEventArgs validationEventArgs)
        {
            Console.WriteLine("{0}: {1}", validationEventArgs.Severity, validationEventArgs.Message);

            if (validationEventArgs.Severity == XmlSeverityType.Error)
                _schemaValidationErrors++;
        }

        private static XmlSchema LoadXmlSchema(string path)
        {
            XmlSchema xs;

            try
            {
                using (var fs = File.OpenRead(path))
                    xs = XmlSchema.Read(fs, null);
            }
            catch (FileNotFoundException)
            {
                throw new XsdFileNotFoundException();
            }

            return xs;
        }
    }
}