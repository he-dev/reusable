using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Utilities.Mailr.Models
{
    [PublicAPI]
    public class Email
    {
        public string From { get; set; } = default!;

        public List<string> To { get; set; } = new List<string>();

        public List<string> CC { get; set; } = new List<string>();

        public bool IsHighPriority { get; set; }

        public string Subject { get; set; } = default!;

        public Dictionary<string, byte[]> Attachments { get; set; } = new Dictionary<string, byte[]>();

        public object Body { get; set; } = default!;

        public bool IsHtml { get; set; }

        public string Theme { get; set; } = default!;

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