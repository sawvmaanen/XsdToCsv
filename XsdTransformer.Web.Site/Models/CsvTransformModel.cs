using System.Collections.Generic;
using System.Web;

namespace XsdTransformer.Web.Site.Models
{
    public class CsvTransformModel
    {
        public IEnumerable<HttpPostedFileBase> Files { get; set; }

        public string XsdFileName { get; set; }

        public string XmlFileName { get; set; }

        public string Guid { get; set; }
    }

    public class CsvTransformViewModel
    {
        public string Csv { get; set; }

        public string ValidationResult { get; set; }
    }
}