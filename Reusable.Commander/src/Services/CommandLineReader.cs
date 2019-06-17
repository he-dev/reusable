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

        public CommandLineReader(ICommandLine commandLine) : this(new CommandArgumentProvider(commandLine)) { }

        public TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> selector)
        {
            var (_, _, itemInfo) = MemberVisitor.GetMemberInfo(selector);
            var itemMetadata = CommandParameterMetadata.Create((PropertyInfo)itemInfo);

            var uri = UriStringHelper.CreateQuery
            (
                scheme: CommandArgumentProvider.DefaultScheme,
                path: ImmutableList<string>.Empty.Add("parameters"),
                query: ImmutableDictionary<string, string>.Empty
                    .Add("name", itemMetadata.Id.Default?.ToString())
                    .AddWhen(itemMetadata.Position.HasValue, "position", itemMetadata.Position?.ToString())
            );

            var metadata =
                ImmutableSession
                    .Empty
                    .SetItem(From<IProviderMeta>.Select(x => x.ProviderName), nameof(CommandArgumentProvider))
                    .SetItem(From<ICommandParameterMeta>.Select(x => x.ParameterType), itemMetadata.Type);

            var item = _args.GetAsync(uri, metadata).GetAwaiter().GetResult();

            if (item.Exists)
            {
                if (itemMetadata.Type.IsList())
                {
                    return item.DeserializeJsonAsync<TValue>().GetAwaiter().GetResult();
                }
                else
                {
                    var values = item.DeserializeJsonAsync<List<TValue>>().GetAwaiter().GetResult();
                    if (((PropertyInfo)itemInfo).PropertyType == typeof(bool))
                    {
                        return
                            values.Any()
                                ? values.Single()
                                : itemMetadata.DefaultValue is TValue defaultValue
                                    ? defaultValue
                                    : (TValue)(object)true;
                    }
                    else
                    {
                        return
                            values.Any()
                                ? values.Single()
                                : itemMetadata.DefaultValue is TValue defaultValue
                                    ? defaultValue
                                    : default;
                    }
                }
            }
            else
            {
                if (itemMetadata.Required)
                {
                    throw DynamicException.Create
                    (
                        $"ParameterNotFound",
                        $"Required parameter '{itemMetadata.Id.First().ToString()}' not specified."
                    );
                }

                if (itemMetadata.DefaultValue is TValue defaultValue)
                {
                    return defaultValue;
                }
            }

            return default;
        }
    }
}