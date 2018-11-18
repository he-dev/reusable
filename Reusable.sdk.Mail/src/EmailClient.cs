using System;
using System.Threading.Tasks;
using Reusable.Reflection;
using Reusable.sdk.Mail.Models;
using Reusable.Validation;

namespace Reusable.sdk.Mail
{
    public interface IEmailClient
    {
        Task SendAsync<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : IEmailSubject
            where TBody : IEmailBody;
    }

    public abstract class EmailClient : IEmailClient
    {
        private static readonly IBouncer<IEmail<IEmailSubject, IEmailBody>> EmailBouncer = Bouncer.For<IEmail<IEmailSubject, IEmailBody>>(builder =>
        {
            builder.Block(e => e.To == null);
            builder.Block(e => e.Subject == null);
            builder.Block(e => e.Body == null);
        });

        public async Task SendAsync<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : IEmailSubject
            where TBody : IEmailBody
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            EmailBouncer
                .Validate((IEmail<IEmailSubject, IEmailBody>)email)
                .ThrowIfInvalid();

            try
            {
                await SendAsyncCore(email);
            }
            catch (Exception innerException)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"SendEmail{nameof(Exception)}",
                    $"Could not send email: Subject = '{email.Subject}' To = '{email.To}' From = '{email.From}'.",
                    innerException
                );
            }
        }

        protected abstract Task SendAsyncCore<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : IEmailSubject
            where TBody : IEmailBody;
    }
}