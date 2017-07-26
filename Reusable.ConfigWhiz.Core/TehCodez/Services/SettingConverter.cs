using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Services
{
    public interface ISettingConverter
    {
        [NotNull]
        T Deserialize<T>([NotNull] object value);

        [NotNull]
        object Serialize([NotNull] object value, [NotNull, ItemNotNull] IImmutableSet<Type> customTypes);
    }

    public abstract class SettingConverter : ISettingConverter
    {
        public T Deserialize<T>(object value)
        {
            return (value is T x) ? x : DeserializeCore<T>(value);
        }

        [NotNull]
        protected abstract T DeserializeCore<T>([NotNull]object value);

        public object Serialize(object value, IImmutableSet<Type> customTypes)
        {
            return customTypes.Contains(value.GetType()) ? value : SerializeCore(value);
        }

        protected abstract object SerializeCore([NotNull]object value);
    }
}