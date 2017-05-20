using System;

namespace Reusable.Colin.Data
{
    public class ArgumentSummary
    {
        public string[] Names { get; set; }

        public string Description { get; set; }

        public Type Type { get; set; }

        public bool Mandatory { get; set; }

        public int Position { get; set; }

        public char ListSeparator { get; set; }
    }
}