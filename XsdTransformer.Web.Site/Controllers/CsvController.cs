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
            return View(new CsvTransformModel());
        }

        [HttpPost]
        public ActionResult Index(CsvTransformModel model)
        {
            if (ModelState.IsValid)
            {
                var folder = Guid.NewGuid().ToString("N");
                var path = EnsureEmptyFolder(folder);
                SaveFiles(model, path);

                return RedirectToAction("Transform", new { folder, model.XsdFileName, model.XmlFileName });
            }

            return View(model);
        }

        public ActionResult Transform(string folder, string xsdFileName, string xmlFileName)
        {
            var result = new CsvTransformViewModel();

            try
            {
                var xsdPath = GetFolderPath(folder + "\\" + xsdFileName);
                var xmlPath = GetFolderPath(folder + "\\" + xmlFileName);
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

        private void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }

            baseDir.Delete(true);
        }

        private void Cleanup(string folder)
        {
            var path = GetFolderPath(folder);

            RecursiveDelete(new DirectoryInfo(path));
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