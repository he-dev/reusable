using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineMapper
    {
        [NotNull]
        TBag Map<TBag>([NotNull] ICommandLine commandLine) where TBag : ICommandBag, new();
    }

    public class CommandLineMapper : ICommandLineMapper
    {
        [PublicAPI] public static readonly ITypeConverter DefaultConverter =
            TypeConverter.Empty
                .Add<StringToSByteConverter>()
                .Add<StringToByteConverter>()
                .Add<StringToCharConverter>()
                .Add<StringToInt16Converter>()
                .Add<StringToInt32Converter>()
                .Add<StringToInt64Converter>()
                .Add<StringToUInt16Converter>()
                .Add<StringToUInt32Converter>()
                .Add<StringToUInt64Converter>()
                .Add<StringToSingleConverter>()
                .Add<StringToDoubleConverter>()
                .Add<StringToDecimalConverter>()
                .Add<StringToColorConverter>()
                .Add<StringToBooleanConverter>()
                .Add<StringToDateTimeConverter>()
                .Add<StringToEnumConverter>()
                .Add<EnumerableToListConverter>();

        private readonly ILogger _logger;
        private readonly ITypeConverter _converter;
        private readonly ConcurrentDictionary<Type, IEnumerable<CommandParameter>> _cache = new ConcurrentDictionary<Type, IEnumerable<CommandParameter>>();

        public CommandLineMapper
        (
            [NotNull] ILogger<CommandLineMapper> logger,
            [NotNull] ITypeConverter converter
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public TBag Map<TBag>(ICommandLine commandLine) where TBag : ICommandBag, new()
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            var parameters = _cache.GetOrAdd(typeof(TBag), bagType => bagType.GetParameters().ToList());
            var bag = new TBag();

            foreach (var parameter in parameters)
            {
                try
                {
                    Map(bag, commandLine, parameter);
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create(
                        $"ParameterMapping",
                        $"Could not map parameter '{parameter.Id.Default.ToString()}'. See inner exception for details.",
                        inner
                    );
                }
            }

            return bag;
        }

        private void Map<TBag>(TBag bag, ICommandLine commandLine, CommandParameter parameter) where TBag : ICommandBag, new()
        {            
            if (commandLine.TryGetArgumentValues(parameter.Id, parameter.Position, out var values))
            {
                if (parameter.Type.IsEnumerableOfT(except: typeof(string)))
                {
                    if (!values.Any())
                    {
                        throw DynamicException.Factory.CreateDynamicException(
                            "EmptyCollection",
                            "Collection parameter and must have at least one value."
                        );
                    }

                    var value = _converter.Convert(values, parameter.Type);
                    parameter.SetValue(bag, value);
                }
                else
                {
                    if (values.Count > 1)
                    {
                        throw DynamicException.Create(
                            "TooManyValues",
                            "Simple parameter must have exactly one value."
                        );
                    }

                    if (parameter.Type == typeof(bool))
                    {
                        if (values.Any())
                        {
                            var value = _converter.Convert(values.Single(), typeof(bool));
                            parameter.SetValue(bag, value);
                        }
                        else
                        {
                            if (parameter.DefaultValue is bool defaultValue)
                            {
                                parameter.SetValue(bag, !defaultValue);
                            }
                            else
                            {
                                // Without a DefaultValue assume false but using the parameter negates it so use true.
                                parameter.SetValue(bag, true);
                            }
                        }
                    }
                    else
                    {
                        var value = _converter.Convert(values.Single(), parameter.Type);
                        parameter.SetValue(bag, value);
                    }
                }
            }
            else
            {
                if (parameter.IsRequired)
                {
                    throw DynamicException.Factory.CreateDynamicException(
                        "MissingValue",
                        "Required parameter must specify a value."
                    );
                }

                if (parameter.DefaultValue.IsNotNull())
                {
                    var value =
                        parameter.DefaultValue is string
                            ? _converter.Convert(parameter.DefaultValue, parameter.Type)
                            : parameter.DefaultValue;

                    parameter.SetValue(bag, value);
                }
            }
        }
    }
}