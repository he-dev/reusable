using System;
using System.Collections.Generic;

namespace Reusable.CommandLine.Data.Help
{
    public class ParameterSummary
    {
        public IEnumerable<string> Names { get; set; }

        public Type Type { get; set; }

        public bool Required { get; set; }

        public int Position { get; set; }

        public string Description { get; set; }
    }
}