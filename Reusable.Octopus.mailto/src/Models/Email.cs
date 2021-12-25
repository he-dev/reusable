using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Translucent.Models
{
    [PublicAPI]
    public interface IEmail<out TSubject, out TBody>
        where TSubject : IEmailSubject
        where TBody : IEmailBody
    {
        string From { get; }

        List<string> To { get; }

        // ReSharper disable once InconsistentNaming - This is by convention and should stay this way.
        List<string> CC { get; }

        bool IsHighPriority { get; }

        TSubject Subject { get; }

        Dictionary<string, byte[]> Attachments { get; }

        TBody Body { get; }

        bool IsHtml { get; }
    }

    public class Email<TSubject, TBody> : IEmail<TSubject, TBody>
        where TSubject : IEmailSubject
        where TBody : IEmailBody
    {
        public string From { get; set; } = default!;

        public List<string> To { get; set; } = new List<string>();

        public List<string> CC { get; set; } = new List<string>();

        public TSubject Subject { get; set; } = default!;

        public Dictionary<string, byte[]> Attachments { get; set; } = new Dictionary<string, byte[]>();

        public TBody Body { get; set; } = default!;

        public bool IsHtml { get; set; }

        public bool IsHighPriority { get; set; }
    }
}