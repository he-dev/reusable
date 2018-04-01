using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class Elapsed : LogAttachement
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public Elapsed() { }

        public Elapsed(string name) : base(name) { }

        public override object Compute(ILog log)
        {
            var innermostElapsed =
                LogScope
                    .Current
                    .Flatten()
                    .Select(scope => scope.TryGetValue(Name, out var value) && value is Elapsed elapsed ? elapsed : null)
                    .Where(Conditional.IsNotNull)
                    .FirstOrDefault();

            return innermostElapsed is null ? null : (object)innermostElapsed._stopwatch.Elapsed;
        }
    }
}