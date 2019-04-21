using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.IOnymous;
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
            var (uri, metadata) = ItemRequestFactory.CreateItemRequest(itemMetadata);

            var item = _args.GetAsync(uri, metadata).GetAwaiter().GetResult();

            if (item.Exists)
            {
                if (itemMetadata.IsCollection)
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

    internal static class CommandLineReaderExtensions
    {
        public static TValue GetItem<TParameter, TValue>(this ICommandLineReader reader, ISelector<TParameter> selector, Expression<Func<TParameter, TValue>> getItem)
        {
            return reader.GetItem<TValue>(getItem);
        }
    }
}