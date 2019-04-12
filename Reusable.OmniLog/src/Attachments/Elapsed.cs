using System.Diagnostics;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Attachments
{
    public class Elapsed : LogAttachment
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