namespace Reusable.Net.Mail
{
    public interface IEmail<out TSubject, out TBody>
        where TSubject : IEmailSubject
        where TBody : IEmailBody
    {
        string From { get; }

        string To { get; }

        bool HighPriority { get; }

        TSubject Subject { get; }

        TBody Body { get; }
    }

    public class Email<TSubject, TBody> : IEmail<TSubject, TBody> 
        where TSubject : IEmailSubject
        where TBody : IEmailBody
    {
        public string From { get; set; }

        public string To { get; set; }

        public bool HighPriority { get; set; }

        public TSubject Subject { get; set; }

        public TBody Body { get; set; }

        public override string ToString() => Body?.ToString() ?? string.Empty;
    }
}
