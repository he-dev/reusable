using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Reusable.Converters.Converters
{
    public class StringToXDocumentConverter : SpecificConverter<String, XDocument>
    {
        public override XDocument Convert(string value, ConversionContext context)
        {
            return XDocument.Parse(value);
        }
    }

    public class XDocumentToStringConverter : SpecificConverter<XDocument, string>
    {
        public override string Convert(XDocument value, ConversionContext context)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var saveMethod = typeof(XDocument).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
                saveMethod.Invoke(value, new object[] { streamWriter, SaveOptions.DisableFormatting });
                var xml = Encoding.UTF8.GetString(memoryStream.ToArray());
                return xml;
            }
        }
    }
}
