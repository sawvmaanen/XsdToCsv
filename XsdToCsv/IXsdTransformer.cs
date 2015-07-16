using System.Collections;
using System.Xml.Linq;

namespace XsdToCsv
{
    public interface IXsdTransformer
    {
        string TransformXml(XDocument document);

        string TransformSchema(ICollection schemas, string objectName, string outputElement);
    }
}