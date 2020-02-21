using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Reusable.OneTo1.Converters
{
    public class StringToXDocumentConverter : TypeConverter<String, XDocument>
    {
        protected override XDocument Convert(string value, ConversionContext context)
        {
            return XDocument.Parse(value);
        }
    }

    public class XDocumentToStringConverter : TypeConverter<XDocument, string>
    {
        protected override string Convert(XDocument value, ConversionContext context)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            var saveMethod = typeof(XDocument).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
            // ReSharper disable once PossibleNullReferenceException - saveMethod is never null
            saveMethod.Invoke(value, new object[] { streamWriter, SaveOptions.DisableFormatting });
            var xml = Encoding.UTF8.GetString(memoryStream.ToArray());
            return xml;
        }
    }
}