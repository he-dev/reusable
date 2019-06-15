using System;
using Reusable.OneTo1;

namespace Reusable.IOnymous
{
    public class UriStringPathToStringConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            return context.Value.Path.Decoded.ToString();
        }
    }
}