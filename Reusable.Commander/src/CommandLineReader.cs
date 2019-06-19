using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineReader<TParameter> where TParameter : ICommandArgumentGroup
    {
        [CanBeNull]
        TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> selector);
    }

    [UsedImplicitly]
    internal class CommandLineReader<TParameter> : ICommandLineReader<TParameter> where TParameter : ICommandArgumentGroup
    {
        private readonly IResourceProvider _arguments;

        public CommandLineReader(IResourceProvider arguments)
        {
            _arguments = arguments;
        }

        public CommandLineReader(ICommandLine commandLine) : this(new CommandArgumentProvider(commandLine)) { }

        public TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> selector)
        {
            var property = selector.ToMemberExpression().Member as PropertyInfo ?? throw new ArgumentException($"{nameof(selector)} must select a property.");
            var argumentMetadata = CommandArgumentMetadata.Create(property);

            var converter = CommandArgumentConverter.Default;

            if (!(argumentMetadata.ConverterType is null))
            {
                converter = converter.Add((ITypeConverter)Activator.CreateInstance(argumentMetadata.ConverterType));
            }
            
            var uri = UriStringHelper.CreateQuery
            (
                scheme: CommandArgumentProvider.DefaultScheme,
                path: ImmutableList<string>.Empty.Add("arguments"),
                query: ImmutableDictionary<string, string>.Empty.Add("name", argumentMetadata.Id.Default.ToString())
            );
            var metadata = ImmutableSession.Empty.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), nameof(CommandArgumentProvider));
            var argument = _arguments.GetAsync(uri, metadata).GetAwaiter().GetResult();

            if (argument.Exists)
            {
                var values = argument.DeserializeBinaryAsync<List<string>>().GetAwaiter().GetResult();

                if (argumentMetadata.Property.PropertyType.IsList())
                {
                    return converter.Convert<TValue>(values);
                }
                else
                {
                    var value = values.SingleOrDefault();

                    if (value is null)
                    {
                        if (argumentMetadata.DefaultValue is TValue defaultValue)
                        {
                            return defaultValue;
                        }

                        if (property.PropertyType == typeof(bool))
                        {
                            return (TValue)(object)true;
                        }
                    }

                    return converter.Convert<TValue>(value);
                }
            }
            else
            {
                return
                    argumentMetadata.DefaultValue is TValue defaultValue
                        ? defaultValue
                        : default;
            }
        }
    }
}