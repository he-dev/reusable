using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.Extensions;

namespace Reusable.SmartConfig
{
    public class UriStringToSettingNameStringConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            return new SettingIdentifier(context.Value);            
        }
    }
}