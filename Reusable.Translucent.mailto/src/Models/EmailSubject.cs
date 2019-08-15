using System.Text;
using JetBrains.Annotations;

namespace Reusable.Translucent.Models
{
    [PublicAPI]
    public interface IEmailSubject
    {
        string Value { get; }

        Encoding Encoding { get; }
    }

    public class EmailSubject : IEmailSubject
    {
        public string Value { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
        
        public static implicit operator EmailSubject(string value) => new EmailSubject { Value = value };
    }
}