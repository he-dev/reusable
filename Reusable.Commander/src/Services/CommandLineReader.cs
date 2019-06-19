using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Collections.Generic;
using Reusable.Quickey;
using Reusable.Reflection;

namespace Reusable.Commander.Services
{
    public interface ICommandLineReader<TParameter> where TParameter : ICommandParameter
    {
        [CanBeNull]
        TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> selector);
    }

    [UsedImplicitly]
    internal class CommandLineReader<TParameter> : ICommandLineReader<TParameter> where TParameter : ICommandParameter
    {
        [PublicAPI]
        public static readonly ITypeConverter Converter =
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

        private readonly IResourceProvider _parameters;

        public CommandLineReader(IResourceProvider parameters)
        {
            _parameters = parameters;
        }

        public CommandLineReader(ICommandLine commandLine) : this(new CommandParameterProvider(commandLine)) { }

        public TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> selector)
        {
            var property = selector.ToMemberExpression().Member as PropertyInfo ?? throw new ArgumentException($"{nameof(selector)} must select a property.");
            var parameterMetadata = CommandParameterMetadata.Create(property);

            var uri = UriStringHelper.CreateQuery
            (
                scheme: CommandParameterProvider.DefaultScheme,
                path: ImmutableList<string>.Empty.Add("parameters"),
                query: ImmutableDictionary<string, string>.Empty.Add("name", parameterMetadata.Id.Default?.ToString())
            );
            var metadata = ImmutableSession.Empty.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), nameof(CommandParameterProvider));
            var parameter = _parameters.GetAsync(uri, metadata).GetAwaiter().GetResult();

            if (parameter.Exists)
            {
                var values = parameter.DeserializeBinaryAsync<List<string>>().GetAwaiter().GetResult();

                if (parameterMetadata.Property.PropertyType.IsList())
                {
                    // if (values.Empty()) throw new ... where are the values?
                    //return parameter.DeserializeJsonAsync<TValue>().GetAwaiter().GetResult();
                    return Converter.Convert<TValue>(values);
                }
                else
                {
                    //var values = parameter.DeserializeJsonAsync<List<TValue>>().GetAwaiter().GetResult();
                    var value = values.SingleOrDefault();

                    if (value is null)
                    {
                        if (parameterMetadata.DefaultValue is TValue defaultValue)
                        {
                            return defaultValue;
                        }

                        if (property.PropertyType == typeof(bool))
                        {
                            return (TValue)(object)true;
                        }
                    }
                    
                    return (TValue)(object)value;
                }
            }
            else
            {
                if (parameterMetadata.Required)
                {
                    throw DynamicException.Create
                    (
                        $"ParameterNotFound",
                        $"Required parameter '{parameterMetadata.Id.First().ToString()}' not specified."
                    );
                }

                if (parameterMetadata.DefaultValue is TValue defaultValue)
                {
                    return defaultValue;
                }
            }

            return default;
        }
    }
}