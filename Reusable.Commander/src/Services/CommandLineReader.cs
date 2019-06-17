using System;
using System.Collections.Generic;
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
    internal interface ICommandLineReader
    {
        [CanBeNull]
        TValue GetItem<TValue>(LambdaExpression getItem);
    }
    
    public interface ICommandLineReader<TParameter> where TParameter : ICommandParameter
    {
        [CanBeNull]
        TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> getItem);
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

        public TValue GetItem<TValue>(Expression<Func<TParameter, TValue>> getItem)
        {
            var (_, _, itemInfo) = MemberVisitor.GetMemberInfo(getItem);
            var itemMetadata = CommandParameterProperty.Create((PropertyInfo)itemInfo);
            var (uri, metadata) = CreateItemRequest(itemMetadata);

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
        
        private static (UriString Uri, IImmutableSession Metadata) CreateItemRequest(CommandParameterProperty item)
        {
            var queryParameters = new (SoftString Key, SoftString Value)[]
            {
                (CommandArgumentQueryStringKeys.Position, item.Position.ToString()),
                (CommandArgumentQueryStringKeys.IsCollection, item.IsCollection.ToString()),
            };
            var path = item.Id.Join("/").ToLower();
            var query =
                queryParameters
                    .Where(x => x.Value)
                    .Select(x => $"{x.Key.ToString()}={x.Value.ToString()}")
                    .Join("&");
            query = (SoftString)query ? $"?{query}" : string.Empty;
            return
            (
                $"{CommandArgumentProvider.DefaultScheme}:///{path}{query}",
                ImmutableSession
                    .Empty
                    .SetItem(From<IProviderMeta>.Select(x => x.ProviderName), nameof(CommandArgumentProvider))
            );
        }
    }

//    internal static class CommandLineReaderExtensions
//    {
//        public static TValue GetItem<TParameter, TValue>(this ICommandLineReader reader, ISelector<TParameter> selector, Expression<Func<TParameter, TValue>> getItem)
//        {
//            return reader.GetItem<TValue>(getItem);
//        }
//    }
}