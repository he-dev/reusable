using System;

namespace Reusable.MQLite.Models
{
    public class PendingMessage
    {
        public long Id { get; set; }

        public byte[] Body { get; set; }

        public byte[] Fingerprint { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}