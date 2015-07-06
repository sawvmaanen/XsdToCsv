using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;

namespace XsdToCsv
{
    public interface IXsdLoader
    {
        ICollection Load(string xsdPath);

        List<KeyValuePair<XmlSeverityType, string>> GetValidationResult();
    }

    public class XsdLoader : IXsdLoader
    {
        private readonly List<KeyValuePair<XmlSeverityType, string>> _validationResult;

        public XsdLoader()
        {
            _validationResult = new List<KeyValuePair<XmlSeverityType, string>>();
        }

        public ICollection Load(string xsdPath)
        {
            var schema = LoadXmlSchema(xsdPath);
            var schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += ValidationEventHandler;
            schemaSet.Add(schema);
            schemaSet.Compile();

            return schemaSet.Schemas();
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