using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace XsdTransformer.Core
{
    public interface IXsdLoader
    {
        XDocument Load(string xsdPath, string xmlPath);

        List<KeyValuePair<XmlSeverityType, string>> GetValidationResult();
    }

    public class XsdLoader : IXsdLoader
    {
        private readonly List<KeyValuePair<XmlSeverityType, string>> _validationResult;

        public XsdLoader()
        {
            _validationResult = new List<KeyValuePair<XmlSeverityType, string>>();
        }

        public XDocument Load(string xsdPath, string xmlPath)
        {
            var settings = new XmlReaderSettings();
            var schema = LoadXmlSchema(xsdPath);
            settings.Schemas.Add(schema);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += ValidationEventHandler;

            using (var reader = XmlReader.Create(xmlPath, settings))
            {
                var document = XDocument.Load(reader, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
                document.Validate(settings.Schemas, ValidationEventHandler, true);
                return document;
            }
        }

        public List<KeyValuePair<XmlSeverityType, string>> GetValidationResult()
        {
            return _validationResult;
        }

        private XmlSchema LoadXmlSchema(string path)
        {
            XmlSchema xs;

            try
            {
                using (var fs = File.OpenRead(path))
                    xs = XmlSchema.Read(fs, null);
            }
            catch (FileNotFoundException)
            {
                throw new XsdFileNotFoundException();
            }

            return xs;
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs validationEventArgs)
        {
            Console.WriteLine("{0}: {1}", validationEventArgs.Severity, validationEventArgs.Message);

            _validationResult.Add(new KeyValuePair<XmlSeverityType, string>(
                validationEventArgs.Severity,
                validationEventArgs.Message));
        }
    }
}