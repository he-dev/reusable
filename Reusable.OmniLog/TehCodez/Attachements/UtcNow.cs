using System;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Computables
{
    public class UtcNow : LogAttachement, IDateTimeProperty
    {
        public override object Compute(Log log) => DateTime.UtcNow;
    }

    public class ConstNow : LogAttachement, IDateTimeProperty
    {
        public override object Compute(Log log) => new DateTime(2017, 5, 1, 10, 10, 30);
    }
}