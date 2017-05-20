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
                if (parameter.Required)
                {
                    if (!arguments.Contains(parameter.Names)) throw new ParameterNotFoundException();
                }
                var arg = arguments[parameter.Names] ?? arguments[parameter.Position];

                if (parameter.Required && arg == null)
            }

            return null;

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

            //new EnumerableObjectToArrayObjectConverter(),
            //new EnumerableObjectToListObjectConverter(),
            //new EnumerableObjectToHashSetObjectConverter(),
            //new DictionaryObjectObjectToDictionaryObjectObjectConverter(),
        });
    }
}
