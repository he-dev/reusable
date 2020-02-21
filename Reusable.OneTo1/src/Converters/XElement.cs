using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Reusable.Extensions;

namespace Reusable.OneTo1.Converters
{
    public class StringToXElement : TypeConverter<String, XElement>
    {
        protected override XElement Convert(string value, ConversionContext context)
        {
            return XElement.Parse(value);
        }
    }

    public class XElementToString : TypeConverter<XElement, String>
    {
        //private static readonly MethodInfo SaveMethod = typeof(XElement).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
        protected override String Convert(XElement value, ConversionContext context)
        {
            var parameter = context.ParameterOrDefault<XmlParameter>();

            using var memoryStream = new MemoryStream();
            //using var streamWriter = new StreamWriter(memoryStream);
            // ReSharper disable once PossibleNullReferenceException - saveMethod is never null
            //SaveMethod.Invoke(value, new object[] { streamWriter, SaveOptions.DisableFormatting });
            value.Save(memoryStream, parameter.SaveOptions);
            return parameter.Encoding.GetString(memoryStream.Rewind().ToArray());
        }
    }

    public class XmlParameter
    {
        public SaveOptions SaveOptions { get; set; } = SaveOptions.None;

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}