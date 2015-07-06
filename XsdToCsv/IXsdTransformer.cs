using System.Collections;

namespace XsdToCsv
{
    public interface IXsdTransformer
    {
        string TransformSchema(ICollection schemas, string objectName, string outputElement);
    }
}