using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Reusable.OneTo1.Converters
{
    public class StringToXElementConverter : TypeConverter<String, XElement>
    {
        protected override XElement Convert(IConversionContext<String> context)
        {
            return XElement.Parse(context.Value);
        }
    }

    public class XElementToStringConverter : TypeConverter<XElement, String>
    {
        protected override String Convert(IConversionContext<XElement> context)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var saveMethod = typeof(XElement).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
                // ReSharper disable once PossibleNullReferenceException - saveMethod is never null
                saveMethod.Invoke(context.Value, new object[] { streamWriter, SaveOptions.DisableFormatting });
                var xml = Encoding.UTF8.GetString(memoryStream.ToArray());
                return xml;
            }
        }
    }
}
