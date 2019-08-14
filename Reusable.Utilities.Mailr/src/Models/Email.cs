using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Utilities.Mailr.Models
{
    [PublicAPI]
    public class Email
    {
        public string From { get; set; }

        public List<string> To { get; set; } = new List<string>();

        public List<string> CC { get; set; } = new List<string>();

        public bool IsHighPriority { get; set; }

        public string Subject { get; set; }

        public Dictionary<string, byte[]> Attachments { get; set; }

        public object Body { get; set; }

        public bool IsHtml { get; set; }

        public string Theme { get; set; }

        public bool CanSend { get; set; } = true;
        
        public class Html : Email
        {
            public Html()
            {
                IsHtml = true;
            }

            public Html(IEnumerable<string> to, string subject) : this()
            {
                To = to.ToList();
                Subject = subject;
            }
        }

        public class Plain : Email
        {
            public Plain()
            {
                IsHtml = false;
            }

            public Plain(IEnumerable<string> to, string subject) : this()
            {
                To = to.ToList();
                Subject = subject;
            }
        }
    }
}