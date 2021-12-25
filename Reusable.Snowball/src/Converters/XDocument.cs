using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Reusable.Essentials.Extensions;

namespace Reusable.Snowball.Converters;

public class StringToXDocument : TypeConverter<String, XDocument>
{
    public LoadOptions LoadOptions { get; set; } = LoadOptions.None;

    protected override XDocument Convert(string value, ConversionContext context)
    {
        return XDocument.Parse(value, LoadOptions);
    }
}

public class XDocumentToString : TypeConverter<XDocument, string>
{
    private static readonly MethodInfo SaveMethod = typeof(XDocument).GetMethod("Save", new[] { typeof(StreamWriter), typeof(SaveOptions) });

    public SaveOptions SaveOptions { get; set; } = SaveOptions.DisableFormatting;

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    protected override string Convert(XDocument value, ConversionContext context)
    {
        using var memoryStream = new MemoryStream();
        //using var streamWriter = new StreamWriter(memoryStream);
        //SaveMethod.Invoke(value, new object[] { streamWriter, SaveOptions });
        value.Save(memoryStream, SaveOptions);
        return Encoding.GetString(memoryStream.Rewind().ToArray());
    }
}