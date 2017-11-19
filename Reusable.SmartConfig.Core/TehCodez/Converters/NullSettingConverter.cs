using System;
using System.Collections.Generic;

namespace Reusable.SmartConfig.Converters
{
    public class NullSettingConverter : ISettingConverter
    {
        public object Deserialize(object value, Type targetType)
        {
            return value;
        }

        public object Serialize(object value, ISet<Type> customTypes)
        {
            return value;
        }
    }
}
