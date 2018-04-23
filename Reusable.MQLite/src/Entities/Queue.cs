using System.Collections.Generic;

namespace Reusable.MQLite.Entities
{
    internal class Queue
    {
        public int Id { get; set; }

        public string Name { get; set; }

        #region Navigation properties

        public List<TimeRange> TimeRanges { get; set; }

        #endregion
    }
}