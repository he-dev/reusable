using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemLog.Attachements
{
    public class AreEqual : LogAttachement
    {
        public override object Compute(Log log)
        {
            var bag = log.Bag();
            if (bag is null)
            {
                return null;
            }
            return bag.TryGetValue("Expected", out var expected) && bag.TryGetValue("Actual", out var actual) ? (object)expected.Equals(actual) : null;
        }
    }
}