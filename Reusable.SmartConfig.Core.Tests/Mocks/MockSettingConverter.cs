using System;
using System.Collections.Generic;

namespace Reusable.SmartConfig.Tests.Mocks
{
    public class MockSettingConverter : ISettingConverter
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
