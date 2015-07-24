using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using FluentValidation.Attributes;
using XsdTransformer.Web.Site.Validators;

namespace XsdTransformer.Web.Site.Models
{
    [Validator(typeof(CsvTransformValidator))]
    public class CsvTransformModel
    {
        [DisplayName("Files")]
        public IEnumerable<HttpPostedFileBase> Files { get; set; }

        [DisplayName("XSD file name")]
        public string XsdFileName { get; set; }

        [DisplayName("XML file name")]
        public string XmlFileName { get; set; }
    }

    public class CsvTransformViewModel
    {
        [DisplayName("CSV")]
        public string Csv { get; set; }

        [DisplayName("Validation result")]
        public string ValidationResult { get; set; }
    }
}