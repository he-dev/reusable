using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Reusable.Marbles.Extensions;

namespace Reusable.Snowball.Converters;

public class StringToXElement : TypeConverter<String, XElement>
{
    protected override XElement Convert(string value, ConversionContext context)
    {
        return XElement.Parse(value);
    }
}

public class XElementToString : TypeConverter<XElement, String>
{
    public SaveOptions SaveOptions { get; set; } = SaveOptions.None;

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    //private static readonly MethodInfo SaveMethod = typeof(XElement).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });
    protected override String Convert(XElement value, ConversionContext context)
    {
        using var memoryStream = new MemoryStream();
        //using var streamWriter = new StreamWriter(memoryStream);
        // ReSharper disable once PossibleNullReferenceException - saveMethod is never null
        //SaveMethod.Invoke(value, new object[] { streamWriter, SaveOptions.DisableFormatting });
        value.Save(memoryStream, SaveOptions);
        return Encoding.GetString(memoryStream.Rewind().ToArray());
    }
}