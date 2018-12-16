using System;
using System.Threading.Tasks;
using Reusable.Exceptionizer;
using Reusable.Flawless;
using Reusable.Reflection;
using Reusable.sdk.Mail.Models;

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
        private static readonly IExpressValidator<IEmail<IEmailSubject, IEmailBody>> EmailValidator = ExpressValidator.For<IEmail<IEmailSubject, IEmailBody>>(builder =>
        {
            builder.False(e => e.To == null);
            builder.False(e => e.Subject == null);
            builder.False(e => e.Body == null);
        });

        public async Task SendAsync<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : IEmailSubject
            where TBody : IEmailBody
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            EmailValidator
                .Validate((IEmail<IEmailSubject, IEmailBody>)email)
                .Assert();

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