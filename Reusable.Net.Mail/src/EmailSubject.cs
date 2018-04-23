using System.Text;

namespace Reusable.Net.Mail
{
    public interface IEmailSubject
    {
        Encoding Encoding { get; }

        string ToString();
    }

    public abstract class EmailSubject : IEmailSubject
    {
        protected EmailSubject()
        {
            Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; set; }

        public abstract override string ToString();
    }

    public class PlainTextSubject : EmailSubject
    {
        private readonly string _text;
        public PlainTextSubject(string text) => _text = text;
        public override string ToString() => _text;
    }
}