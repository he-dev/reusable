using System;

namespace SmartUtilities.TypeFramework.Converters
{
    public class StringToUriConverter : TypeConverter<String, Uri>
    {
        public override Uri Convert(string value, ConversionContext context)
        {
            return new Uri(value);
        }
    }
}
