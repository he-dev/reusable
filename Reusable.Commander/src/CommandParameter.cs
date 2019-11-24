using System.Diagnostics;
using Reusable.Diagnostics;

namespace Reusable.Commander
{
    // foo -bar -baz qux
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandParameter
    {
        public bool Async { get; set; }
    }
}