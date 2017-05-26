using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Validators;
using Reusable.Extensions;
using Reusable.TypeConversion;
using Reusable.Colin.Data;

namespace Reusable.Colin.Services
{
    public class CommandParameterFactory : IEnumerable<CommandParameter>
    {
        [CanBeNull]
        private readonly IImmutableList<CommandParameter> _parameters;

        private readonly TypeConverter _converter;

        public CommandParameterFactory([CanBeNull] Type parameterType)
        {
            ParameterType = parameterType;
            if (parameterType == null) return;

            if (!parameterType.HasDefaultConstructor()) throw new ArgumentException($"The '{nameof(parameterType)}' '{parameterType}' must have a default constructor.");

            _parameters = CreateParameters(parameterType).ToImmutableList();
            _converter = DefaultConverter;

            ParameterValidator.ValidateParameterNamesUniqueness(_parameters);
            ParameterValidator.ValidateParameterPositions(_parameters);
        }

        [PublicAPI]
        [CanBeNull]
        public Type ParameterType { get; }

        [PublicAPI]
        [NotNull]
        [ItemNotNull]
        internal static IEnumerable<CommandParameter> CreateParameters(Type parameterType)
        {
            return
                parameterType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                    .Select(p => new CommandParameter(p));
        }

        public object CreateParameter(ArgumentLookup arguments, CultureInfo culture)
        {
            if (ParameterType == null) return null;

            var instance = Activator.CreateInstance(ParameterType);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var parameter in _parameters)
            {
                if (!arguments.Contains(parameter))
                {
                    if (parameter.Required)
                    {
                        throw new ParameterNotFoundException(parameter.Name);
                    }
                    //continue;
                }

                var values = arguments.Parameter(parameter).ToList();

                if (TryGetParameterData(parameter, values, out (object data, Type dataType) result))
                {
                    var value = _converter.Convert(result.data, result.dataType, null, CultureInfo.InvariantCulture);
                    parameter.Property.SetValue(instance, value);
                }
            }

            return instance;
        }

        private static bool TryGetParameterData([NotNull] CommandParameter commandParameter, [NotNull] ICollection<string> values, out (object data, Type dataType) result)
        {
            // Boolean paramater need special treatment because their value is optional. 
            // Just setting the flag means that its default value should be used.
            if (commandParameter.Property.PropertyType == typeof(bool))
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
                    if (commandParameter.Property.PropertyType.IsEnumerable())
                    {
                        result.data = values;
                        result.dataType = commandParameter.Property.PropertyType;
                        return true;
                    }

                    result.data = values.Single();
                    result.dataType = commandParameter.Property.PropertyType;
                    return true;
                }
            }

            // Still didn't find the value. Try to use the default one.
            if (commandParameter.DefaultValue != null)
            {
                result.data = commandParameter.DefaultValue;
                result.dataType = commandParameter.DefaultValue.GetType();
                return true;
            }

            result.data = null;
            result.dataType = null;
            return false;

        }

        #region  IEnumerable

        public IEnumerator<CommandParameter> GetEnumerator() => _parameters?.GetEnumerator() ?? Enumerable.Empty<CommandParameter>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [PublicAPI]
        public static readonly TypeConverter DefaultConverter = TypeConverter.Empty.Add(
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
            new EnumerableToArrayConverter());
    }

    public class ParameterNotFoundException : Exception
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public ParameterNotFoundException([NotNull] ImmutableNameSet parameterNames)
            : base(parameterNames.ToString())
        { }
    }
}
