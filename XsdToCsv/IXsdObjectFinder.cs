using System.Collections;
using System.Xml.Schema;

namespace XsdToCsv
{
    public interface IXsdObjectFinder
    {
        XmlSchemaComplexType FindSchemaType(IEnumerable schemas, string name);

        XmlSchemaElement FindSchemaElement(IEnumerable schemas, string name);
    }
}