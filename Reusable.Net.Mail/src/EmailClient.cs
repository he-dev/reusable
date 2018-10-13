using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Reusable.Reflection;
using Reusable.Validation;

namespace Reusable.Net.Mail
{
    public interface IEmailClient
    {
        Task SendAsync<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : IEmailSubject
            where TBody : IEmailBody;
    }

    public abstract class EmailClient : IEmailClient
    {
        private static readonly IWeelidator<IEmail<IEmailSubject, IEmailBody>> EmailWeelidator = Weelidator.For<IEmail<IEmailSubject, IEmailBody>>(builder =>
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

            EmailWeelidator
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