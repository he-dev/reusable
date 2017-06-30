using System.Collections.Generic;
using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
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