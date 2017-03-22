using System;

namespace Reusable.Windows
{
    public class CoerceValueEventArgs<TValue> : EventArgs
    {
        internal CoerceValueEventArgs(TValue baseValue)
        {
            NewValue = baseValue;
            CoercedValue = baseValue;
        }
        public TValue NewValue { get; }
        public TValue CoercedValue { get; set; }
        public bool Canceled { get; set; }
    }
}