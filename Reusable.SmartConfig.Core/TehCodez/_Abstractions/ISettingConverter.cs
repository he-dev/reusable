using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    public interface ISettingConverter
    {
        [NotNull]
        object Deserialize([NotNull] object value, [NotNull] Type targetType);

        [NotNull]
        object Serialize([NotNull] object value);
    }
}