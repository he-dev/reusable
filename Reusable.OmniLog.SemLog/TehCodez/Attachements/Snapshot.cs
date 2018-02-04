using System;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions.Attachements
{
    public class Snapshot : LogAttachement
    {
        private readonly IStateSerializer _serializer;

        /// <summary>
        /// Creates a new Snapshot attachement. Uses JsonStateSerializer by default.
        /// </summary>
        public Snapshot(IStateSerializer serializer = null) : base(nameof(Snapshot))
        {
            _serializer = serializer ?? new JsonStateSerializer();
        }

        public override object Compute(ILog log)
        {
            //if (log.TryGetValue(nameof(LogBag), out var value) && value is LogBag bag && bag.TryGetValue(Name.ToString(), out var obj))
            //{
            //    return _serializer.SerializeObject(obj);
            //}

            return
                log.TryGetValue(Name + nameof(Object), out var snapshot) 
                    ? _serializer.SerializeObject(snapshot) 
                    : null;
        }
    }
}