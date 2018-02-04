using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class Elapsed : LogAttachement
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public Elapsed() { }

        public Elapsed(string name) : base(name) { }

        public TimeSpan Value => _stopwatch.Elapsed;

        public override object Compute(ILog log)
        {
            return _stopwatch.Elapsed;
        }
    }

    public class Scope : LogAttachement
    {
        private readonly IStateSerializer _serializer;

        public Scope([NotNull] IStateSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public override object Compute(ILog log)
        {
            var states =
                LogScope
                    .Current
                    .Flatten()
                    .Select(scope => new
                    {
                        Scope = scope.Name,
                        Context = scope["State"]
                    });

            return _serializer.SerializeObject(states);
        }
    }
}