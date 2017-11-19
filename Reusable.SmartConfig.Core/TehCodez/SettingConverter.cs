using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    public interface ISettingConverter
    {
        [NotNull]
        object Deserialize([NotNull] object value, [NotNull] Type targetType);

        [NotNull]
        object Serialize([NotNull] object value, [NotNull, ItemNotNull] ISet<Type> toTypes);
    }

    public abstract class SettingConverter : ISettingConverter
    {
        public object Deserialize(object value, Type targetType)
        {
            if (value.GetType() == targetType)
            {
                return value;
            }

            if (value is string)
            {
                return DeserializeCore(value, targetType);
            }

            throw new ArgumentException($"Unsupported type: {value.GetType().Name}");
        }

        [NotNull]
        protected abstract object DeserializeCore([NotNull]object value, [NotNull] Type toType);

        public object Serialize(object value, ISet<Type> toTypes)
        {
            return toTypes.Contains(value.GetType()) ? value : SerializeCore(value);
        }

        [NotNull]
        protected abstract object SerializeCore([NotNull]object value);
    }
}