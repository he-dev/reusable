using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Reusable.Converters
{
    public class StringToXElementConverter : TypeConverter<String, XElement>
    {
        protected override XElement ConvertCore(IConversionContext<String> context)
        {
            return XElement.Parse(context.Value);
        }
    }

    public class XElementToStringConverter : TypeConverter<XElement, String>
    {
        protected override String ConvertCore(IConversionContext<XElement> context)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var saveMethod = typeof(XElement).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
                saveMethod.Invoke(context.Value, new object[] { streamWriter, SaveOptions.DisableFormatting });
                var xml = Encoding.UTF8.GetString(memoryStream.ToArray());
                return xml;
            }
        }
    }
}
