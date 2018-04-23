using System;
using System.Collections.Generic;

namespace Reusable.MQLite.Entities
{
    internal class TimeRange
    {
        public int Id { get; set; }

        public int QueueId { get; set; }

        public DateTime StartsOn { get; set; }

        public DateTime EndsOn { get; set; }

        public DateTime CreatedOn { get; set; }

        #region Navigation properties

        public Queue Queue { get; set; }

        public List<Message> Messages { get; set; }

        #endregion
    }
}