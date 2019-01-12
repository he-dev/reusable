using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
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
                To = to,
                Subject = subject,
                Body = body,
            };

            configureEmail(email);

            return email;
        }
    }
}
