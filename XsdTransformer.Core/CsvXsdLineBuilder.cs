using System.Globalization;
using System.Linq;

namespace XsdTransformer.Core
{
    public class CsvXsdLineBuilder : IXsdLineBuidler
    {
        private readonly string _indent;
        private readonly string _separator;

        public CsvXsdLineBuilder(string indent, string separator)
        {
            _indent = indent;
            _separator = separator;
        }

        public string BuildHeader()
        {
            var headers = new[] { "Elementnaam", "Multi", "Type", "Omschrijving" };
            return headers.Aggregate("", (c, n) => c + (string.IsNullOrWhiteSpace(c) ? "" : _separator) + n) + "\n";
        }

        public string BuildLine(int level, string name, decimal minOccurs, decimal maxOccurs, string typeName, string example = "")
        {
            var strMaxOccures = maxOccurs == decimal.MaxValue ? "*" : maxOccurs.ToString(CultureInfo.InvariantCulture);
            var strMinOccurs = minOccurs == 0 && strMaxOccures == "*" ? "*" : minOccurs.ToString(CultureInfo.InvariantCulture);
            var format = strMinOccurs != strMaxOccures ? "{0}..{1}" : "{0}";
            var occurs = string.Format(format, strMinOccurs, strMaxOccures);

            name = string.Concat(Enumerable.Repeat(_indent, level)) + name;
            return string.Format("{1}{0}{2}{0}{3}{0}{4}\n", _separator, Quoted(name), Quoted(occurs), Quoted(typeName), Quoted(example));
        }

        private string Quoted(string value, string quote = "\"")
        {
            return string.Format("{0}{1}{0}", string.IsNullOrEmpty(value) ? "" : quote, value);
        }
    }
}