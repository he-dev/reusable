using System;
using JetBrains.Annotations;

namespace Reusable.sdk.Mailr
{
    public class Email<TBody>
    {
        public string To { get; set; }

        public string Subject { get; set; }

        public TBody Body { get; set; }

        public bool IsHtml { get; set; } = true;

        public string Theme { get; set; }

        public bool CanSend { get; set; } = true;
    }

    public class Email
    {
        public static Email<TBody> CreateHtml<TBody>(string to, string subject, TBody body, [CanBeNull] Action<Email<TBody>> configureEmail = null)
        {
            return Create(to, subject, body, email =>
            {
                email.IsHtml = true;
                configureEmail?.Invoke(email);
            });
        }

        public static Email<TBody> CreatePlain<TBody>(string to, string subject, TBody body, [CanBeNull] Action<Email<TBody>> configureEmail = null)
        {
            return Create(to, subject, body, email =>
            {
                email.IsHtml = false;
                configureEmail?.Invoke(email);
            });
        }

        internal static Email<TBody> Create<TBody>([NotNull] string to, [NotNull] string subject, [NotNull] TBody body, [NotNull] Action<Email<TBody>> configureEmail)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (configureEmail == null) throw new ArgumentNullException(nameof(configureEmail));

            var email = new Email<TBody>
            {
                To = to,
                Subject = subject,
                Body = body,
            };

            configureEmail.Invoke(email);

            return email;
        }
    }
}
