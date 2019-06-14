using System.Text;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    public class UriStringToStringConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            // config:settings?name=BASE64STRING
            var uri = context.Value;
            var nameBytes = System.Convert.FromBase64String(uri.Query["name"].ToString());
            return Encoding.UTF8.GetString(nameBytes);
        }
    }
}