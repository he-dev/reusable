using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class Elapsed : LogAttachement
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public Elapsed() { }

        public Elapsed(string name) : base(name) { }

        [NotNull]
        public override object Compute(Log log)
        {
            return _stopwatch.Elapsed;
        }
    }
}