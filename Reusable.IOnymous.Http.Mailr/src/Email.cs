using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class Email<TBody>
    {
        public string From { get; set; }

        public List<string> To { get; set; } = new List<string>();

        public List<string> CC { get; set; } = new List<string>();

        public bool IsHighPriority { get; set; }

        public string Subject { get; set; }

        public Dictionary<string, byte[]> Attachments { get; set; }

        public TBody Body { get; set; }

        public bool IsHtml { get; set; }

        public string Theme { get; set; }

        public bool CanSend { get; set; } = true;
    }

    public static class Email
    {
        public static Email<TBody> CreateHtml<TBody>(IEnumerable<string> to, string subject, TBody body, [CanBeNull] Action<Email<TBody>> configureEmail = null)
        {
            return Create(to, subject, body, email =>
            {
                email.IsHtml = true;
                configureEmail?.Invoke(email);
            });
        }

        public static Email<TBody> CreatePlain<TBody>(IEnumerable<string> to, string subject, TBody body, [CanBeNull] Action<Email<TBody>> configureEmail = null)
        {
            return Create(to, subject, body, email =>
            {
                email.IsHtml = false;
                configureEmail?.Invoke(email);
            });
        }

        internal static Email<TBody> Create<TBody>(IEnumerable<string> to, [NotNull] string subject, [NotNull] TBody body, [NotNull] Action<Email<TBody>> configureEmail)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (configureEmail == null) throw new ArgumentNullException(nameof(configureEmail));

            var email = new Email<TBody>
            {
                To = to.ToList(),
                Subject = subject,
                Body = body,
            };

            configureEmail(email);

            return email;
        }
    }
}