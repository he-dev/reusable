using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IEmail<out TSubject, out TBody>
        where TSubject : IEmailSubject
        where TBody : IEmailBody
    {
        string From { get; }

        IEnumerable<string> To { get; }

        IEnumerable<string> CC { get; }

        bool IsHighPriority { get; }

        TSubject Subject { get; }

        TBody Body { get; }

        bool IsHtml { get; }
    }

    public class Email<TSubject, TBody> : IEmail<TSubject, TBody>
        where TSubject : IEmailSubject
        where TBody : IEmailBody
    {
        public string From { get; set; }

        public IEnumerable<string> To { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> CC { get; set; } = Enumerable.Empty<string>();

        public TSubject Subject { get; set; }

        public TBody Body { get; set; }

        public bool IsHtml { get; set; }

        public bool IsHighPriority { get; set; }
    }

    [PublicAPI]
    public interface IEmailSubject
    {
        string Value { get; }

        Encoding Encoding { get; }
    }

    public class EmailSubject : IEmailSubject
    {
        public string Value { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }

    [PublicAPI]
    public interface IEmailBody
    {
        string Value { get; }

        Encoding Encoding { get; }
    }

    public class EmailBody : IEmailBody
    {
        public string Value { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}