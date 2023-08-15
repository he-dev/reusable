using System.Collections.Generic;

namespace Reusable.Wiretap.Data;

public class SoftDictionary<TValue> : Dictionary<string, TValue>
{
    public SoftDictionary() : base(SoftString.Comparer) { }
}