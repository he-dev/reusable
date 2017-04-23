using System;

namespace Reusable
{
    public interface IEmailClient
    {
        void Send<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody;
    }

    public abstract class EmailClient : IEmailClient
    {
        public void Send<TSubject, TBody>(IEmail<TSubject, TBody> email) where TSubject : EmailSubject where TBody : EmailBody
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            if (email.To == null) throw new InvalidOperationException($"You need to set {nameof(Email<TSubject, TBody>)}.{nameof(IEmail<TSubject, TBody>.To)} first.");
            if (email.Subject == null) throw new InvalidOperationException($"You need to set {nameof(Email<TSubject, TBody>)}.{nameof(IEmail<TSubject, TBody>.Subject)} first.");
            if (email.Body == null) throw new InvalidOperationException($"You need to set {nameof(Email<TSubject, TBody>)}.{nameof(IEmail<TSubject, TBody>.Body)} first.");

            try
            {
                SendCore(email);
            }
            catch (Exception innerException)
            {
                throw new SendEmailException<TSubject, TBody>(email, innerException);
            }
        }

        protected abstract void SendCore<TSubject, TBody>(IEmail<TSubject, TBody> email) where TSubject : EmailSubject where TBody : EmailBody;
    }

    public class SendEmailException<TSubject, TBody>
        : Exception
        where TSubject : EmailSubject
        where TBody : EmailBody
    {
        public SendEmailException(IEmail<TSubject, TBody> email, Exception innerException)
            : base($"Could not send the email '{email.Subject}' to '{email.To}' from '{email.From}'.", innerException)
        { }
    }
}
