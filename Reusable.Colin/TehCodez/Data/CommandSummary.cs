using System.Collections.Generic;

namespace Reusable.Colin.Data
{
    public class CommandSummary
    {
        public IEnumerable<string> Names { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }
    }
}