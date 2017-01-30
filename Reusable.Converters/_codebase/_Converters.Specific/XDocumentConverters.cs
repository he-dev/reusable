using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Reusable.Converters
{
    public class StringToXDocumentConverter : TypeConverter<String, XDocument>
    {
        protected override XDocument ConvertCore(IConversionContext<String> context)
        {
            return XDocument.Parse(context.Value);
        }
    }

    public class XDocumentToStringConverter : TypeConverter<XDocument, string>
    {
        protected override string ConvertCore(IConversionContext<XDocument> context)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var saveMethod = typeof(XDocument).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
                saveMethod.Invoke(context.Value, new object[] { streamWriter, SaveOptions.DisableFormatting });
                var xml = Encoding.UTF8.GetString(memoryStream.ToArray());
                return xml;
            }
        }
    }
}
