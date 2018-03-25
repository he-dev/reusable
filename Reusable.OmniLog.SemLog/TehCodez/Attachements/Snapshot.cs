using System;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions.Attachements
{
    public class Snapshot : LogAttachement
    {
        private readonly ISerializer _serializer;

        /// <summary>
        /// Creates a new Snapshot attachement. Uses JsonStateSerializer by default.
        /// </summary>
        public Snapshot(ISerializer serializer = null) : base(nameof(Snapshot))
        {
            _serializer = serializer ?? new JsonSerializer();
        }

        public override object Compute(ILog log)
        {
            return
                log.TryGetValue(Name + nameof(Object), out var snapshot) 
                    ? _serializer.Serialize(snapshot) 
                    : null;
        }
    }
}