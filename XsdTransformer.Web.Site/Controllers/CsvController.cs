using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Schema;
using XsdTransformer.Core;
using XsdTransformer.Web.Site.Models;

namespace XsdTransformer.Web.Site.Controllers
{
    public class CsvController : Controller
    {
        private readonly IXsdLoader _loader;

        private readonly IXsdTransformer _transformer;

        public CsvController(IXsdLoader loader, IXsdTransformer transformer)
        {
            _loader = loader;
            _transformer = transformer;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Transform(CsvTransformModel model)
        {
            var result = new CsvTransformViewModel();

            try
            {
                var folder = GetTransformHash(model);

                if (string.IsNullOrWhiteSpace(model.Guid))
                {
                    var path = EnsureEmptyFolder(folder);
                    SaveFiles(model, path);
                }

                var xsdPath = GetFolderPath(folder + "\\" + model.XsdFileName);
                var xmlPath = GetFolderPath(folder + "\\" + model.XmlFileName);
                var document = _loader.Load(xsdPath, xmlPath);
                var validationResult = _loader.GetValidationResult();

                Cleanup(folder);
                result.ValidationResult = validationResult.Aggregate("", (c, n) => string.Format("{0}\n{1}", c, n));

                if (_loader.GetValidationResult().All(x => x.Key != XmlSeverityType.Error))
                    result.Csv = _transformer.TransformXml(document);
            }
            catch (Exception ex)
            {
                result.ValidationResult = ex.ToString();
            }

            return View(result);
        }

        private void Cleanup(string folder)
        {
            var path = GetFolderPath(folder);

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private string GetTransformHash(CsvTransformModel model)
        {
            return model.Guid ?? Guid.NewGuid().ToString("N");
        }

        private void SaveFiles(CsvTransformModel model, string path)
        {
            foreach (var file in model.Files)
            {
                using (var sr = new StreamReader(file.InputStream))
                {
                    var content = sr.ReadToEnd();

                    // Set any import file references within the XSD to newly created path.
                    content = Regex.Replace(content, "schemaLocation=\"(.+)?\"", string.Format("schemaLocation=\"file:///{0}/$1\"", path.Replace("\\", "/")));

                    using (var sw = System.IO.File.CreateText(Path.Combine(path, file.FileName)))
                        sw.Write(content);
                }
            }
        }

        private string GetFolderPath(string name)
        {
            return Path.Combine(Server.MapPath("~/App_Data/"), name);
        }

        private string EnsureEmptyFolder(string name)
        {
            var path = GetFolderPath(name);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
            return path;
        }
    }
}