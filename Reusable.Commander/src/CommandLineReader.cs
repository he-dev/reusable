using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineReader : IEnumerable<CommandArgument>
    {
        [CanBeNull]
        TValue GetItem<TValue>(Expression<Func<TValue>> selector);
    }

    [UsedImplicitly]
    internal class CommandLineReader : ICommandLineReader
    {
        private readonly IDictionary<NameSet, CommandArgument> _arguments;
        
        public CommandLineReader(IDictionary<NameSet, CommandArgument> arguments)
        {
            _arguments = arguments;
        }

        public TValue GetItem<TValue>(Expression<Func<TValue>> selector)
        {
            var property = selector.ToMemberExpression().Member as PropertyInfo ?? throw new ArgumentException($"{nameof(selector)} must select a property.");
            var argumentMetadata = CommandArgumentMetadata.Create(property);

            var converter = CommandArgumentConverter.Default;

            if (!(argumentMetadata.ConverterType is null))
            {
                converter = converter.Add((ITypeConverter)Activator.CreateInstance(argumentMetadata.ConverterType));
            }
            

            if (_arguments.TryGetValue(argumentMetadata.Name, out var values))
            {
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
        
        #region IEnumerable

        public IEnumerator<CommandArgument> GetEnumerator() => _arguments.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}