using System;

namespace Reusable.MQLite.Models
{
    public class TimeRange
    {
        public int Id { get; set; }

        public DateTime StartsOn { get; set; }

        public DateTime EndsOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }    
}