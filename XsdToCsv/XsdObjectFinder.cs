using System.Collections;
using System.Xml.Schema;

namespace XsdToCsv
{
    public class XsdObjectFinder : IXsdObjectFinder
    {
        public XmlSchemaComplexType FindSchemaType(IEnumerable schemas, string name)
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

        public XmlSchemaElement FindSchemaElement(IEnumerable schemas, string name)
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
    }
}