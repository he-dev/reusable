using System;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    /// <summary>
    /// Serliazes log-scope where the innermost scope is first, then other sopes follow in the reversed order.
    /// </summary>
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
                        scope.Name,
                        Context = scope[LogProperties.State]
                    });

            return _serializer.SerializeObject(states);
        }
    }
}