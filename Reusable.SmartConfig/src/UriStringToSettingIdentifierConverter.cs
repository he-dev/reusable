using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    public class UriStringToSettingIdentifierConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            return new SettingIdentifier(context.Value);
        }
    }
}