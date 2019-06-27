using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Deception.Triggers
{
    [UsedImplicitly]
    public class CountPattern : PhantomExceptionPattern<int>
    {
        private int _counter;

        public CountPattern(IEnumerable<int> values) : base(values) { }

//        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
//        {
//            //b.DisplayValue(x => x._counter);
//            //b.DisplayValue(x => x.Current);
//        });

        protected override bool Matches() => ++_counter == Current;

        protected override void Reset() => _counter = 0;

        public override string ToString() => $"{nameof(CountPattern)}: {_counter}";
    }
}