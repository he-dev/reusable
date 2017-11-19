using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using Reusable.Exceptionize;
using Reusable.Flawless;

namespace Reusable.Net.Mail
{
    public interface IEmailClient
    {
        void Send<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody;
    }

    public abstract class EmailClient : IEmailClient
    {
        private static readonly Validator<IEmail<EmailSubject, EmailBody>> EmailValidator =
            Validator<IEmail<EmailSubject, EmailBody>>.Empty
                .IsNotValidWhen(e => e.To == null)
                .IsNotValidWhen(e => e.Subject == null)
                .IsNotValidWhen(e => e.Body == null);
        
        public void Send<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            
            ((IEmail<EmailSubject, EmailBody>)email).ValidateWith(EmailValidator).ThrowIfNotValid();
            
            try
            {
                SendCore(email);
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

        protected abstract void SendCore<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody;
    }
}
