namespace Reusable.Net.Mail
{
    public interface IEmail<TSubject, TBody>
        where TSubject : EmailSubject
        where TBody : EmailBody
    {
        string From { get; set; }

        string To { get; set; }

        bool HighPriority { get; set; }

        TSubject Subject { get; set; }

        TBody Body { get; set; }
    }

    public class Email<TSubject, TBody> : IEmail<TSubject, TBody> 
        where TSubject : EmailSubject
        where TBody : EmailBody
    {
        public string From { get; set; }

        public string To { get; set; }

        public bool HighPriority { get; set; }

        public TSubject Subject { get; set; }

        public TBody Body { get; set; }

        public override string ToString() => Body?.ToString() ?? string.Empty;
    }
}
