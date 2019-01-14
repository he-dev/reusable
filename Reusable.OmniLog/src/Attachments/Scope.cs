using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.OmniLog.Attachments
{
    /// <summary>
    /// Serializes log-scope where the innermost scope is first, then other scopes follow in the reversed order.
    /// </summary>
    public class Scope : LogAttachment
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
                    .Select(IgnoreAttachments)
                    .ToList();

            return
                scopes.Any()
                    ? _serializer.Serialize(scopes)
                    : default;
        }

        private static IEnumerable<KeyValuePair<SoftString, object>> IgnoreAttachments(ILog scope)
        {
            return
                from x in scope
                where !(x.Value is ILogAttachment)
                orderby x.Key
                select x;
        }
    }
}