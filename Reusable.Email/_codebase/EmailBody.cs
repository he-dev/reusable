using System;
using System.Text;

namespace Reusable
{
    public abstract class EmailBody
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