using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;

namespace Reusable.Snowball;

// This converter diverges from the pure composite pattern and does not use the branch/leaf structure 
// because with it converters need to be registered in a specific order if they depend on one another.
[PublicAPI]
public class TypeConverterStack : ITypeConverter, IEnumerable<ITypeConverter>
{
    // We cannot use a dictionary because there are no unique keys. Generic types can have multiple matches.
    private readonly Stack<ITypeConverter> _converters;

    public TypeConverterStack() => _converters = new Stack<ITypeConverter>();

    public TypeConverterStack(IEnumerable<ITypeConverter> converters) : this()
    {
        converters =
            from c in converters
            from x in c as IEnumerable<ITypeConverter> ?? new[] { c }
            where x is {}
            select c;

        _converters.PushRange(converters);
    }

    public TypeConverterStack(params ITypeConverter[] converters) : this(converters.AsEnumerable()) { }

    public static ITypeConverter Empty => new TypeConverterStack();

    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
    {
        foreach (var converter in this)
        {
            if (converter.ConvertOrDefault(value, toType, context) is {} result)
            {
                return result;
            }
        }

        return default;
    }

    public void Add(ITypeConverter converter) => _converters.Add(converter.Decorate());

    public IEnumerator<ITypeConverter> GetEnumerator() => _converters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}