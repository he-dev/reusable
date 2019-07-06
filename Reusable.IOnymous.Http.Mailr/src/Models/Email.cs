using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Http.Mailr.Models
{
    [PublicAPI]
    public class Email
    {
        public Email()
        {
            CreateSerializeStreamFunc = () => ResourceHelper.SerializeAsJsonAsync(this);
        }

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

        [JsonIgnore]
        public Func<Task<Stream>> CreateSerializeStreamFunc { get; set; }

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