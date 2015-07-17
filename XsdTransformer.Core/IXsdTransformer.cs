using System.Xml.Linq;

namespace XsdTransformer.Core
{
    public interface IXsdTransformer
    {
        string TransformXml(XDocument document);
    }
}