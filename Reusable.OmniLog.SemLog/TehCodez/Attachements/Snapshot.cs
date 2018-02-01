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

        public override object Compute(Log log)
        {
            if (log.TryGetValue(nameof(LogBag), out var bag) && bag is LogBag b && b.TryGetValue(Name.ToString(), out var obj))
            {
                return _serializer.SerializeObject(obj);
            }
            return null;
        }
    }
}