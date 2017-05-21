using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.TypeConversion;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Validators;
using Reusable.Extensions;

namespace Reusable.Colin
{
    public class ParameterFactory
    {
        [CanBeNull]
        private readonly Type _parameterType;
        [CanBeNull]
        private readonly IImmutableList<Data.ParameterInfo> _parameters;
        private readonly TypeConverter _converter;

        public ParameterFactory([CanBeNull] Type parameterType)
        {
            _parameterType = parameterType;
            if (parameterType == null) return;

            if (!parameterType.HasDefaultConstructor()) throw new ArgumentException($"The '{nameof(parameterType)}' '{parameterType}' must have a default constructor.");

            _parameters = CreateParameters(parameterType).ToImmutableList();
            _converter = DefaultConverter;

            ParameterValidator.ValidateParameterNamesUniqueness(_parameters);
        }

        [NotNull]
        [ItemNotNull]
        internal static IEnumerable<Data.ParameterInfo> CreateParameters(Type parameterType)
        {
            return
                parameterType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                    .Select(p => new Data.ParameterInfo(p));
        }

        public object CreateParameter(ArgumentLookup arguments)
        {
            if (_parameterType == null) return null;

            var instance = Activator.CreateInstance(_parameterType);

            foreach (var parameter in _parameters)
            {
                if (parameter.Required && !arguments.Contains(parameter))
                {
                    throw new ParameterNotFoundException(parameter.Names);
                }

                var values = arguments.Parameter(parameter).ToList();

                if (TryGetParameterData(parameter, values, out (object data, Type dataType) result))
                {
                    var value = _converter.Convert(result.data, result.dataType);
                    parameter.Property.SetValue(instance, value);
                }
            }

            return instance;
        }

        private static bool TryGetParameterData([NotNull] Data.ParameterInfo parameter, [NotNull] ICollection<string> values, out (object data, Type dataType) result)
        {
            // Boolean paramater need special treatment because their value is optional. 
            // Just setting the flag means that its default value should be used.
            if (parameter.Property.PropertyType == typeof(bool))
            {
                var value = values.SingleOrDefault();
                if (value != null)
                {
                    result.data = value;
                    result.dataType = typeof(bool);
                    return true;
                }
            }
            else
            {
                if (values.Any())
                {
                    if (parameter.Property.PropertyType.IsEnumerable())
                    {
                        result.data = values;
                        result.dataType = parameter.Property.PropertyType;
                        return true;
                    }

                    result.data = values.Single();
                    result.dataType = parameter.Property.PropertyType;
                    return true;
                }
            }

            if (parameter.DefaultValue != null)
            {
                result.data = parameter.DefaultValue;
                result.dataType = parameter.DefaultValue.GetType();
                return true;
            }

            result.data = null;
            result.dataType = null;
            return false;

        }

        private TypeConverter DefaultConverter => TypeConverter.Empty.Add(new TypeConverter[]
        {
            new StringToSByteConverter(),
            new StringToByteConverter(),
            new StringToCharConverter(),
            new StringToInt16Converter(),
            new StringToInt32Converter(),
            new StringToInt64Converter(),
            new StringToUInt16Converter(),
            new StringToUInt32Converter(),
            new StringToUInt64Converter(),
            new StringToSingleConverter(),
            new StringToDoubleConverter(),
            new StringToDecimalConverter(),
            new StringToColorConverter(),
            new StringToBooleanConverter(),
            new StringToDateTimeConverter(),
            new StringToEnumConverter(),

            new EnumerableToArrayConverter()
            //new EnumerableObjectToArrayObjectConverter(),
            //new EnumerableObjectToListObjectConverter(),
            //new EnumerableObjectToHashSetObjectConverter(),
            //new DictionaryObjectObjectToDictionaryObjectObjectConverter(),
        });
    }
}
