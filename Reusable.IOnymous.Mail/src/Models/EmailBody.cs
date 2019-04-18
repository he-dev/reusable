using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.IOnymous
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
    }
}