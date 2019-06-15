using System;
using Reusable.OneTo1;

namespace Reusable.IOnymous.Config
{
    public class UriStringQueryNameToStringConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(context.Value.Query["name"].ToString());
        }
    }
}