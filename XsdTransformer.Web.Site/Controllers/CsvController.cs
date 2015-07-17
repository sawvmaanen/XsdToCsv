using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Schema;
using WebGrease.Css.Extensions;
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
            var folder = GetTransformHash(model);

            if (string.IsNullOrWhiteSpace(model.Guid))
            {
                var path = EnsureEmptyFolder(folder);

                SaveFiles(model, path);
            }

            var oldCurrentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(GetFolderPath(folder));

            var result = new CsvTransformViewModel();
            var document = _loader.Load(model.XsdFileName, model.XmlFileName);
            var validationResult = _loader.GetValidationResult();

            Directory.SetCurrentDirectory(oldCurrentDir);

            result.ValidationResult = validationResult.Aggregate("", (c, n) => string.Format("{0}\n{1}", c, n));

            if (_loader.GetValidationResult().All(x => x.Key != XmlSeverityType.Error))
                result.Csv = _transformer.TransformXml(document);

            return View(result);
        }

        private string GetTransformHash(CsvTransformModel model)
        {
            return model.Guid ?? Guid.NewGuid().ToString("N");
        }

        private void SaveFiles(CsvTransformModel model, string path)
        {
            model.Files.ForEach(x => x.SaveAs(Path.Combine(path, x.FileName)));
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