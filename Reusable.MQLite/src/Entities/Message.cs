using System;

namespace Reusable.MQLite.Entities
{
    internal class Message
    {
        public long Id { get; set; }

        public int TimeRangeId { get; set; }

        public byte[] Body { get; set; }

        public byte[] Fingerprint { get; set; }

        public DateTime? DeletedOn { get; set; }

        #region Navigation properties

        public TimeRange TimeRange { get; set; }

        #endregion
    }
}