using System;
using System.Diagnostics;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class ElapsedMilliseconds : Elapsed
    {
        public ElapsedMilliseconds() { }

        public ElapsedMilliseconds(string name) : base(name)
        {

        }

        public override object Compute(ILog log)
        {
            var innermostElapsed =
                LogScope
                    .Current
                    .Flatten()
                    .Select(scope => scope.TryGetValue(Name, out var value) && value is Elapsed elapsed ? elapsed : null)
                    .Where(Conditional.IsNotNull)
                    .FirstOrDefault();

            //return (long)((TimeSpan)base.Compute(log)).TotalMilliseconds;
            return innermostElapsed is null ? null : (object)(long)innermostElapsed.Value.TotalMilliseconds;//Compute(log);
        }
    }
}