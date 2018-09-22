using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.Converters.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineMapper
    {
        TParameter Map<TParameter>([NotNull] ICommandLine commandLine) where TParameter : ICommandBag, new();
    }

    public class CommandLineMapper : ICommandLineMapper
    {
        [PublicAPI]
        public static readonly ITypeConverter DefaultConverter =
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

        public CommandLineMapper(
            [NotNull] ILogger<CommandLineMapper> logger,
            [NotNull] ITypeConverter converter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public TParameterBag Map<TParameterBag>(ICommandLine commandLine) where TParameterBag : ICommandBag, new()
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            var parameters = _cache.GetOrAdd(typeof(TParameterBag), GetParameters<TParameterBag>());
            var bag = new TParameterBag();

            foreach (var parameter in parameters)
            {
                var longName = parameter.Name.FirstLongest().ToString();

                if (commandLine.Contains(parameter.Name))
                {
                    var values = commandLine.Values(parameter).ToList();

                    if (parameter.Type.IsEnumerable(ignore: typeof(string)))
                    {
                        if (!values.Any())
                        {
                            throw DynamicException.Factory.CreateDynamicException(
                                $"{parameter.Name.FirstLongest().ToString()}ParameterException",
                                $"Collection parameters must have at leas one value.");
                        }

                        var value = _converter.Convert(values, parameter.Type);
                        parameter.SetValue(bag, value);
                    }
                    else
                    {
                        if (values.Count > 1)
                        {
                            throw DynamicException.Factory.CreateDynamicException(
                                $"{parameter.Name.FirstLongest().ToString()}ParameterException",
                                $"Simple parameters must not have more than one value.");
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
                        throw ($"{longName}ParameterNotFound{nameof(Exception)}", $"{longName} is required.").ToDynamicException();
                    }

                    if (parameter.DefaultValue.IsNotNull())
                    {
                        parameter.SetValue(bag, parameter.DefaultValue);
                        //parameter.SetValue(bag, _converter.Convert(parameter.DefaultValue, parameter.Type));
                    }
                }
            }

            return bag;
        }

        private static IEnumerable<CommandParameter> GetParameters<T>()
        {
            return
                typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    //.Where(property => property.IsDefined(typeof(ParameterAttribute)))
                    .Select(CommandParameter.Create)
                    .ToList();
        }
    }
}