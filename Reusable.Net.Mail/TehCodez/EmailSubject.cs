using System.Text;

namespace Reusable.Net.Mail
{
    public abstract class EmailSubject
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