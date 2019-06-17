using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Collections.Generic;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineMapper
    {
        [NotNull]
        TBag Map<TBag>([NotNull] ICommandLine commandLine) where TBag : ICommandParameter; //, new();
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
        private readonly ConcurrentDictionary<Type, IEnumerable<CommandParameterMetadata>> _cache = new ConcurrentDictionary<Type, IEnumerable<CommandParameterMetadata>>();

        public CommandLineMapper
        (
            [NotNull] ILogger<CommandLineMapper> logger,
            [NotNull] ITypeConverter converter
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public TBag Map<TBag>(ICommandLine commandLine) where TBag : ICommandParameter//, new()
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            var parameters = _cache.GetOrAdd(typeof(TBag), bagType => bagType.GetParameters().ToList());
            var bag = Activator.CreateInstance<TBag>();

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

        private void Map<TBag>(TBag bag, ICommandLine commandLine, CommandParameterMetadata parameterMetadata) where TBag : ICommandParameter//, new()
        {            
            if (commandLine.TryGetArgumentValues(parameterMetadata.Id, parameterMetadata.Position, out var values))
            {
                if (parameterMetadata.Type.IsEnumerableOfT(except: typeof(string)))
                {
                    if (!values.Any())
                    {
                        throw DynamicException.Factory.CreateDynamicException(
                            "EmptyCollection",
                            "Collection parameter and must have at least one value."
                        );
                    }

                    var value = _converter.Convert(values, parameterMetadata.Type);
                    parameterMetadata.SetValue(bag, value);
                }
                else
                {
                    if (values.Count > 1)
                    {
                        throw DynamicException.Create
                        (
                            "TooManyValues",
                            "Simple parameter must have exactly one value."
                        );
                    }

                    if (parameterMetadata.Type == typeof(bool))
                    {
                        if (values.Any())
                        {
                            var value = _converter.Convert(values.Single(), typeof(bool));
                            parameterMetadata.SetValue(bag, value);
                        }
                        else
                        {
                            if (parameterMetadata.DefaultValue is bool defaultValue)
                            {
                                parameterMetadata.SetValue(bag, !defaultValue);
                            }
                            else
                            {
                                // Without a DefaultValue assume false but using the parameter negates it so use true.
                                parameterMetadata.SetValue(bag, true);
                            }
                        }
                    }
                    else
                    {
                        var value = _converter.Convert(values.Single(), parameterMetadata.Type);
                        parameterMetadata.SetValue(bag, value);
                    }
                }
            }
            else
            {
                if (parameterMetadata.Required)
                {
                    throw DynamicException.Factory.CreateDynamicException(
                        "MissingValue",
                        "Required parameter must specify a value."
                    );
                }

                if (parameterMetadata.DefaultValue.IsNotNull())
                {
                    var value =
                        parameterMetadata.DefaultValue is string
                            ? _converter.Convert(parameterMetadata.DefaultValue, parameterMetadata.Type)
                            : parameterMetadata.DefaultValue;

                    parameterMetadata.SetValue(bag, value);
                }
            }
        }
    }
}