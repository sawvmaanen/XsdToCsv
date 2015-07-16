using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;

namespace XsdToCsv
{
    public class XsdTransformer : IXsdTransformer
    {
        private readonly Dictionary<string, XmlSchemaComplexType> _describedTypes;

        private readonly IXsdObjectFinder _objectFinder;
        private readonly IXsdLineBuidler _lineBuidler;

        public XsdTransformer(IXsdObjectFinder objectFinder, IXsdLineBuidler lineBuidler)
        {
            _objectFinder = objectFinder;
            _lineBuidler = lineBuidler;
            _describedTypes = new Dictionary<string, XmlSchemaComplexType>();
        }

        public string TransformXml(XDocument document)
        {
            return _lineBuidler.BuildHeader() + TransformElement(0, document.Root);
        }

        private string TransformElement(int level, XElement element)
        {
            var info = element.GetSchemaInfo();

            if (info == null)
                return null;

            var schemaElement = info.SchemaElement;
            var minOccurs = schemaElement != null ? schemaElement.MinOccurs : 0;
            var maxOccurs = schemaElement != null ? schemaElement.MaxOccurs : decimal.MaxValue;
            var type = string.Empty;
            var description = "";
            var example = element.HasElements ? "" : element.Value;

            if (schemaElement != null)
            {
                var complexType = schemaElement.ElementSchemaType as XmlSchemaComplexType;
                type = complexType != null ? complexType.Name : schemaElement.ElementSchemaType.TypeCode.ToString();

                var documentation = GetDocumentation(schemaElement);
                if (string.IsNullOrWhiteSpace(documentation))
                    description = example;
                else
                    description = documentation + (!documentation.Contains("ENUMERATION: ") ? " (EXAMPLE: " + example + ")" : "");
            }

            var line = _lineBuidler.BuildLine(level, element.Name.LocalName, minOccurs, maxOccurs, type, description);

            foreach (var child in element.Elements())
                line += TransformElement(level + 1, child);

            return line;
        }

        public string TransformSchema(ICollection schemas, string objectName, string outputElement)
        {
            var schemaType = _objectFinder.FindSchemaType(schemas, objectName);
            var header = _lineBuidler.BuildHeader();

            if (schemaType == null)
            {
                var element = _objectFinder.FindSchemaElement(schemas, objectName);
                if (element == null)
                    throw new XsdSchemaOrElementNotFoundException();
                return header + TransformElement(0, element);
            }
            return header + TransformSchemaType(0, schemaType, outputElement, 1, 1);
        }

        public string TransformElement(int level, XmlSchemaElement element)
        {
            var documentation = GetDocumentation(element);
            var result = _lineBuidler.BuildLine(level, element.QualifiedName.Name, element.MinOccurs, element.MaxOccurs, element.ElementSchemaType.TypeCode.ToString(), documentation);
            return result + TransformComplexType(level + 1, element.SchemaType);
        }

        private string TransformSchemaType(int level, XmlSchemaType schemaType, string outputElement, decimal minOccurs, decimal maxOccurs)
        {
            var result = _lineBuidler.BuildLine(level, outputElement, minOccurs, maxOccurs, schemaType.Name);
            return result + TransformComplexType(level + 1, schemaType);
        }

        private string TransformComplexType(int level, XmlSchemaType schemaType)
        {
            var result = "";
            var complexType = schemaType as XmlSchemaComplexType;

            if (complexType != null && !_describedTypes.ContainsKey(complexType.QualifiedName.ToString()))
            {
                if (!string.IsNullOrWhiteSpace(complexType.QualifiedName.ToString()))
                    _describedTypes.Add(complexType.QualifiedName.ToString(), complexType);

                var sequence = complexType.ContentTypeParticle as XmlSchemaSequence;

                if (sequence != null)
                    foreach (var item in sequence.Items)
                    {
                        var childElement = item as XmlSchemaElement;

                        if (childElement != null)
                        {
                            var childComplexType = childElement.ElementSchemaType as XmlSchemaComplexType;

                            if (childComplexType != null)
                                result += TransformSchemaType(level, childComplexType, childElement.QualifiedName.Name, childElement.MinOccurs, childElement.MaxOccurs);
                            else
                                result += TransformElement(level, childElement);
                        }
                    }
            }
            return result;
        }

        private string GetDocumentation(XmlSchemaElement element)
        {
            var doc = "";

            if (element.Annotation != null)
            {
                doc = element.Annotation.Items.OfType<XmlSchemaDocumentation>()
                    .SelectMany(x => x.Markup)
                    .Aggregate("", (c, n) => c + n.Value);
                doc = Regex.Replace(doc, @"[\n+|\t+]", " ");
                doc = Regex.Replace(doc, @"\s+", " ").Trim();
            }

            var simpleType = element.ElementSchemaType as XmlSchemaSimpleType;
            if (simpleType == null)
                return null;

            var restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            if (restriction != null && restriction.Facets.Cast<XmlSchemaFacet>().Any(x => x is XmlSchemaEnumerationFacet))
            {
                doc += " ENUMERATION: ";
                doc += restriction.Facets.Cast<XmlSchemaFacet>()
                    .Where(x => x is XmlSchemaEnumerationFacet).Select(x => x.Value).ToList()
                    .Aggregate("", (c, n) => c + (string.IsNullOrWhiteSpace(c) ? "" : ", ") + n);
            }

            return doc.Trim();
        }
    }
}