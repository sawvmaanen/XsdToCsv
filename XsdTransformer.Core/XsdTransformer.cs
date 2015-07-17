using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;

namespace XsdTransformer.Core
{
    public class XsdTransformer : IXsdTransformer
    {
        private readonly IXsdLineBuidler _lineBuidler;

        public XsdTransformer(IXsdLineBuidler lineBuidler)
        {
            _lineBuidler = lineBuidler;
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