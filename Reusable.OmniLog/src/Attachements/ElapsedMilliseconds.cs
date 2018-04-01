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

        /// <summary>
        /// Creates a new instance of ElapsedMilliseconds and allows to override the default name which is "ElapsedMilliseconds".
        /// </summary>
        public ElapsedMilliseconds(string name) : base(name)
        {

        }

        public override object Compute(ILog log)
        {
            var elapsed = base.Compute(log);
            return elapsed is null ? null : (object)(long)((TimeSpan)elapsed).TotalMilliseconds;
        }
    }
}