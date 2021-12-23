﻿using System.Collections.Generic;
using System.Diagnostics;
using Reusable.Diagnostics;

namespace Reusable.Commander
{
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandParameter
    {
        public bool Async { get; set; }

        public List<string> Params { get; set; } = new List<string>();
    }
}