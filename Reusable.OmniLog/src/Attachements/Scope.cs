using System;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    /// <summary>
    /// Serliazes log-scope where the innermost scope is first, then other sopes follow in the reversed order.
    /// </summary>
    public class Scope : LogAttachement
    {
        private readonly ISerializer _serializer;

        public Scope([NotNull] ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public override object Compute(ILog log)
        {
            var scopes =
                LogScope
                    .Current
                    .Flatten()
                    .Cast<ILogScope>()
                    .ToList();

            return 
                scopes.Any() 
                    ? _serializer.Serialize(scopes) 
                    : default;
        }
    }
}