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
        private readonly IResourceProvider _args;

        public CommandLineReader(IResourceProvider args)
        {
            _args = args;
        }

        public CommandLineReader(ICommandLine commandLine) : this(new CommandParameterProvider(commandLine)) { }

        public TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> selector)
        {
            var property = selector.ToMemberExpression().Member as PropertyInfo ?? throw new ArgumentException($"{nameof(selector)} must select a property.");
            var parameter = CommandParameterMetadata.Create(property);

            var uri = UriStringHelper.CreateQuery
            (
                scheme: CommandParameterProvider.DefaultScheme,
                path: ImmutableList<string>.Empty.Add("parameters"),
                query: ImmutableDictionary<string, string>.Empty.Add("name", parameter.Id.Default?.ToString())
            );

            var metadata =
                ImmutableSession
                    .Empty
                    .SetItem(From<IProviderMeta>.Select(x => x.ProviderName), nameof(CommandParameterProvider))
                    .SetItem(From<ICommandParameterMeta>.Select(x => x.ParameterType), parameter.Property.PropertyType);

            var item = _args.GetAsync(uri, metadata).GetAwaiter().GetResult();

            if (item.Exists)
            {
                if (parameter.Property.PropertyType.IsList())
                {
                    return item.DeserializeJsonAsync<TValue>().GetAwaiter().GetResult();
                }
                else
                {
                    var values = item.DeserializeJsonAsync<List<TValue>>().GetAwaiter().GetResult();
                    if (property.PropertyType == typeof(bool))
                    {
                        return
                            values.Any()
                                ? values.Single()
                                : parameter.DefaultValue is TValue defaultValue
                                    ? defaultValue
                                    : (TValue)(object)true;
                    }
                    else
                    {
                        return
                            values.Any()
                                ? values.Single()
                                : parameter.DefaultValue is TValue defaultValue
                                    ? defaultValue
                                    : default;
                    }
                }
            }
            else
            {
                if (parameter.Required)
                {
                    throw DynamicException.Create
                    (
                        $"ParameterNotFound",
                        $"Required parameter '{parameter.Id.First().ToString()}' not specified."
                    );
                }

                if (parameter.DefaultValue is TValue defaultValue)
                {
                    return defaultValue;
                }
            }

            return default;
        }
    }
}