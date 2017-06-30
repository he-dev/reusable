using System.Collections.Generic;
using Reusable.ConfigWhiz.Data.Annotations;

namespace Reusable.ConfigWhiz.Tests.Common.Configurations
{
    public class CollectionConfiguration
    {        
        public List<int> JsonArray { get; set; }

        [Itemized]
        public int[] ArrayInt32 { get; set; }

        [Itemized]
        public Dictionary<string, int> DictionaryStringInt32 { get; set; }
    }
}