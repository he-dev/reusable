using System.Text;

namespace Reusable.Net.Mail
{
    public interface IEmailBody
    {
        bool IsHtml { get; }

        Encoding Encoding { get; }

        string ToString();
    }

    public abstract class EmailBody : IEmailBody
    {
        protected EmailBody() => Encoding = Encoding.UTF8;

        public Encoding Encoding { get; set; }

        public bool IsHtml { get; set; }

        public abstract override string ToString();
    }

    public class PlainTextBody : EmailBody
    {
        private readonly string _text;
        public PlainTextBody(string text) => _text = text;
        public override string ToString() => _text;
    }
}