using System;
using JetBrains.Annotations;
using Reusable.OneTo1;

namespace Reusable.IOnymous.Config
{
    [PublicAPI]
    public class UriStringQueryToStringConverter : TypeConverter<UriString, string>
    {
        private readonly string _key;

        public UriStringQueryToStringConverter(string key)
        {
            _key = key;
        }

        public UriStringQueryToStringConverter() : this("name") { }

        public static ITypeConverter<UriString, string> Default { get; } = new UriStringQueryToStringConverter();

        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(context.Value.Query[_key].ToString());
        }
    }
}