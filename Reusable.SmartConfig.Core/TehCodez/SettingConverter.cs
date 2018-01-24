using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    public abstract class SettingConverter : ISettingConverter
    {
        private readonly ISet<Type> _supportedTypes;

        private readonly Type _fallbackType;

        protected SettingConverter(IEnumerable<Type> supportedTypes)
        {
            _supportedTypes = new HashSet<Type>(supportedTypes ?? throw new ArgumentNullException(nameof(supportedTypes)));
            _fallbackType = supportedTypes.FirstOrDefault() ?? throw new ArgumentException("There must be at least one supprted type.");
        }

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

        public object Serialize(object value)
        {
            var targetType = 
                _supportedTypes.Contains(value.GetType()) 
                    ? value.GetType() 
                    : _fallbackType;

            return SerializeCore(value, targetType);
        }

        [NotNull]
        protected abstract object SerializeCore([NotNull]object value, [NotNull] Type targetType);
    }
}