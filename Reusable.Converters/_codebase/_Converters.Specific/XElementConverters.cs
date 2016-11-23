using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Reusable.Converters
{
    public class StringToXElementConverter : SpecificConverter<String, XElement>
    {
        public override XElement Convert(string value, ConversionContext context)
        {
            return XElement.Parse(value);
        }
    }

    public class XElementToStringConverter : SpecificConverter<XElement, String>
    {
        public override String Convert(XElement value, ConversionContext context)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var saveMethod = typeof(XElement).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
                saveMethod.Invoke(value, new object[] { streamWriter, SaveOptions.DisableFormatting });
                var xml = Encoding.UTF8.GetString(memoryStream.ToArray());
                return xml;
            }
        }
    }
}
