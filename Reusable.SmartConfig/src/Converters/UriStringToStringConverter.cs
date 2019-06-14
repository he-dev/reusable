using System;
using System.Text;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    public class UriStringToStringConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(context.Value.Query["name"].ToString());
        }
    }
}