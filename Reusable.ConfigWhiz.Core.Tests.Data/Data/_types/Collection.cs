using System;
using System.Collections.Generic;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz.Tests.Common.Data
{
    [TypeConverter(typeof(JsonToObjectConverter<List<Int32>>))]
    [TypeConverter(typeof(ObjectToJsonConverter<List<Int32>>))]
    public class Collection
    {        
        public List<int> JsonArray { get; set; }

        [Itemized]
        public int[] ArrayInt32 { get; set; }

        [Itemized]
        public Dictionary<string, int> DictionaryStringInt32 { get; set; }
    }
}