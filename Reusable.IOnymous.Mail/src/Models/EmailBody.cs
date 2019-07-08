using System.Text;
using JetBrains.Annotations;

namespace Reusable.IOnymous.Mail
{
    [PublicAPI]
    public interface IEmailBody
    {
        string Value { get; }

        Encoding Encoding { get; }
    }

    public class EmailBody : IEmailBody
    {
        public string Value { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public static implicit operator EmailBody(string value) => new EmailBody { Value = value };
    }
}