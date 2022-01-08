using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Synergy.Requests;

[PublicAPI]
public abstract class SendMail : Request<Unit>
{
    public string From { get; set; } = default!;
    public List<string> To { get; set; } = new();
    public List<string> CC { get; set; } = new();
    public string Subject { get; set; } = default!;
    public Dictionary<string, byte[]> Attachments { get; set; } = new();
    public bool IsHtml { get; set; }
    public bool IsHighPriority { get; set; }
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public object Body { get; set; } = default!;
}

[PublicAPI]
public class SendMailWithSmtp : SendMail
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
}