using System;
using System.Collections.Generic;

namespace Reusable.Utilities.JsonNet.Abstractions;

public interface ITypeDictionary : IEnumerable<KeyValuePair<string, Type>>
{
    Type? Resolve(string typeName);
}