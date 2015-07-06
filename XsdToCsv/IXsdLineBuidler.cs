namespace XsdToCsv
{
    public interface IXsdLineBuidler
    {
        string BuildHeader();

        string BuildLine(int level, string name, decimal minOccurs, decimal maxOccurs, string typeName, string example = "");
    }
}