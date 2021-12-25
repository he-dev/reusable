using System.Collections.Generic;
using System.Diagnostics;
using Reusable.Essentials.Diagnostics;

namespace Reusable.DoubleDash;

[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public class CommandParameter
{
    public bool Async { get; set; }

    public List<string> Params { get; set; } = new List<string>();
}