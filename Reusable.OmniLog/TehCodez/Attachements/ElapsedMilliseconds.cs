using System;
using System.Diagnostics;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class ElapsedMilliseconds : Elapsed
    {
        public ElapsedMilliseconds() { }

        public ElapsedMilliseconds(string name) : base(name)
        {

        }
        public override object Compute(Log log)
        {
            return (long)((TimeSpan)base.Compute(log)).TotalMilliseconds;
        }
    }
}