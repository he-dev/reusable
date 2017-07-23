using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.CommandLine.Services
{
    public abstract class CommandParameterFactory
    {
        public static ParameterMetadata CreateCommandParameterMetadata([CanBeNull] Type parameterType)
        {
            if (parameterType == null)
            {
                return new ParameterMetadata(null, Enumerable.Empty<ArgumentMetadata>());
            }

            if (!parameterType.HasDefaultConstructor())
            {
                throw new ArgumentException($"The '{nameof(parameterType)}' '{parameterType}' must have a default constructor.");
            }

            var argumentMetadata = CreateParameters(parameterType).ToImmutableList();

            ParameterValidator.ValidateParameterNamesUniqueness(argumentMetadata);
            ParameterValidator.ValidateParameterPositions(argumentMetadata);

            return new ParameterMetadata(parameterType, argumentMetadata);
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<ArgumentMetadata> CreateParameters([NotNull] Type parameterType)
        {
            return
                parameterType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                    .Select(p => new ArgumentMetadata(ImmutableNameSetFactory.CreateParameterNameSet(p), p));
        }

        public static object CreateParameter(ParameterMetadata parameter, ConsoleContext context)
        {
            if (parameter.ParameterType == null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(parameter.ParameterType);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var property in parameter)
            {
                if (!context.Arguments.Contains(property))
                {
                    if (property.Required)
                    {
                        throw new ParameterNotFoundException(property.Name);
                    }
                    //continue;
                }

                var values = context.Arguments.Parameter(property).ToList();

                if (TryGetParameterData(property, values, out (object data, Type dataType) result))
                {
                    var value = DefaultConverter.Convert(result.data, result.dataType, null, context.Culture);
                    property.Property.SetValue(instance, value);
                }
            }

            if (instance is ConsoleCommandParameter consoleParameter)
            {
                consoleParameter.Commands = context.Commands;
                consoleParameter.Logger = context.Logger;
            }

            return instance;
        }

        private static bool TryGetParameterData([NotNull] ArgumentMetadata argument, [NotNull] ICollection<string> values, out (object data, Type dataType) result)
        {
            // Boolean paramater need special treatment because their value is optional. 
            // Just setting the flag means that its default value should be used.
            if (argument.Property.PropertyType == typeof(bool))
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
                    if (argument.Property.PropertyType.IsEnumerable())
                    {
                        result.data = values;
                        result.dataType = argument.Property.PropertyType;
                        return true;
                    }

                    result.data = values.Single();
                    result.dataType = argument.Property.PropertyType;
                    return true;
                }
            }

            // Still didn't find the value. Try to use the default one.
            if (argument.DefaultValue != null)
            {
                result.data = argument.DefaultValue;
                result.dataType = argument.DefaultValue.GetType();
                return true;
            }

            result.data = null;
            result.dataType = null;
            return false;

        }
        

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
        public ParameterNotFoundException([NotNull] IImmutableNameSet parameterNames)
            : base(parameterNames.ToString())
        { }
    }
}
