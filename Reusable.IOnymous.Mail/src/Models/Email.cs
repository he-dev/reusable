using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.IOnymous
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
        public string From { get; set; }

        public List<string> To { get; set; } = new List<string>();

        public List<string> CC { get; set; } = new List<string>();

        public TSubject Subject { get; set; }

        public Dictionary<string, byte[]> Attachments { get; set; }

        public TBody Body { get; set; }

        public bool IsHtml { get; set; }

        public bool IsHighPriority { get; set; }
    }
}