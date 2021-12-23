using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Essentials;

namespace Reusable.Utilities.JsonNet.Annotations;

using static AttributeTargets;
    
[UsedImplicitly]
[AttributeUsage(Class | Interface)]
public class JsonTypeSchemaAttribute : Attribute, IEnumerable<string>
{
    private readonly string _name;

    public JsonTypeSchemaAttribute(string name) => _name = name;

    public string? Alias { get; set; }

    public override string ToString() => _name;

    public IEnumerator<string> GetEnumerator()
    {
        yield return _name;
        if (Alias.IsNotNullOrEmpty()) yield return Alias!;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}