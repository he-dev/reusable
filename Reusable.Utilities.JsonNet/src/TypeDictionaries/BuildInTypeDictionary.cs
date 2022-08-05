using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Marbles;
using Reusable.Utilities.JsonNet.Abstractions;

namespace Reusable.Utilities.JsonNet.TypeDictionaries;

public class BuildInTypeDictionary : ITypeDictionary
{
    /// <summary>
    /// Gets a dictionary of primitive types.
    /// </summary>
    private static readonly IImmutableDictionary<string, Type> BuiltInTypes =
        ImmutableDictionary
            .Create<string, Type>(SoftString.Comparer)
            .Add("bool", typeof(Boolean))
            .Add("byte", typeof(Byte))
            .Add("sbyte", typeof(SByte))
            .Add("char", typeof(Char))
            .Add("decimal", typeof(Decimal))
            .Add("double", typeof(Double))
            .Add("float", typeof(Single))
            .Add("int", typeof(Int32))
            .Add("uint", typeof(UInt32))
            .Add("long", typeof(Int64))
            .Add("ulong", typeof(UInt64))
            .Add("object", typeof(Object))
            .Add("short", typeof(Int16))
            .Add("ushort", typeof(UInt16))
            .Add("string", typeof(String));

    public Type? Resolve(string typeName)
    {
        return BuiltInTypes.TryGetValue(typeName, out var type) ? type : Type.GetType(typeName, ignoreCase: true, throwOnError: false);
    }

    public static readonly ITypeDictionary Default = new BuildInTypeDictionary();

    public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => BuiltInTypes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)BuiltInTypes).GetEnumerator();
}