using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Translucent.Data
{
    [PublicAPI]
    public abstract class MailRequest : Request
    {
        public string From { get; set; } = default!;
        public List<string> To { get; set; } = new List<string>();
        public List<string> CC { get; set; } = new List<string>();
        public string Subject { get; set; } = default!;
        public Dictionary<string, byte[]> Attachments { get; set; } = new Dictionary<string, byte[]>();
        public bool IsHtml { get; set; }
        public bool IsHighPriority { get; set; }
    }

    [PublicAPI]
    public class SmtpRequest : MailRequest
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public bool UseSsl { get; set; }
    }

    public class SmtpResponse : Response { }
}