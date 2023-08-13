using System.Collections.Generic;

namespace Reusable.Wiretap.Data;

public class SoftDictionary : Dictionary<string, object?>
{
    public SoftDictionary() : base(SoftString.Comparer) { }
}