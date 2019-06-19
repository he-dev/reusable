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
            var values__ = parameter.DeserializeBinaryAsync<List<string>>().GetAwaiter().GetResult();

            if (parameter.Exists)
            {
                if (parameterMetadata.Property.PropertyType.IsList())
                {
                    return parameter.DeserializeJsonAsync<TValue>().GetAwaiter().GetResult();
                }
                else
                {
                    var values = parameter.DeserializeJsonAsync<List<TValue>>().GetAwaiter().GetResult();
                    if (property.PropertyType == typeof(bool))
                    {
                        return
                            values.Any()
                                ? values.Single()
                                : parameterMetadata.DefaultValue is TValue defaultValue
                                    ? defaultValue
                                    : (TValue)(object)true;
                    }
                    else
                    {
                        return
                            values.Any()
                                ? values.Single()
                                : parameterMetadata.DefaultValue is TValue defaultValue
                                    ? defaultValue
                                    : default;
                    }
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