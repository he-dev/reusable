using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.FormatProviders;
using Reusable.Reflection;

namespace Reusable.SmartConfig
{
    using static FormattableStringHelper;

    public interface ISettingConverter
    {
        [NotNull]
        object Deserialize([NotNull] object value, [NotNull] Type targetType);

        [NotNull]
        object Serialize([NotNull] object value);
    }
    
    public abstract class SettingConverter : ISettingConverter
    {
        private readonly ISet<Type> _supportedTypes;

        private readonly Type _fallbackType;

        protected SettingConverter(IEnumerable<Type> supportedTypes, Type fallbackType)
        {
            _supportedTypes = new HashSet<Type>(supportedTypes ?? throw new ArgumentNullException(nameof(supportedTypes)));
            _fallbackType = fallbackType ?? throw new ArgumentException("There must be at least one supported type.");
        }

        public object Deserialize(object value, Type targetType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            try
            {
                return
                    value.GetType() == targetType
                        ? value
                        : DeserializeCore(value, targetType);
            }
            catch (Exception ex)
            {
                throw DynamicException.Create(nameof(Deserialize), Format($"Error converting '{value.GetType()}' to '{targetType}'. See the inner exception for details.", TypeFormatProvider.Default), ex);
            }
        }

        [NotNull]
        protected abstract object DeserializeCore([NotNull] object value, [NotNull] Type targetType);

        public object Serialize(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var targetType =
                _supportedTypes.Contains(value.GetType())
                    ? value.GetType()
                    : _fallbackType;

            try
            {
                return
                    value.GetType() == targetType
                        ? value
                        : SerializeCore(value, targetType);
            }
            catch (Exception ex)
            {
                throw DynamicException.Create(nameof(Serialize), Format($"Error converting '{value.GetType()}' to '{targetType}'. See the inner exception for details.", TypeFormatProvider.Default), ex);
            }
        }

        [NotNull]
        protected abstract object SerializeCore([NotNull] object value, [NotNull] Type targetType);
    }

    /// <summary>
    /// This converter does nothing and passes the value as is.
    /// </summary>
    public class RelaySettingConverter : ISettingConverter
    {
        public object Deserialize(object value, Type targetType) => value;

        public object Serialize(object value) => value;
    }
}