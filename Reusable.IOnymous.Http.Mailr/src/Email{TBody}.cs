using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class Email<TBody>
    {
        public IEnumerable<string> To { get; set; }

        public IEnumerable<string> CC { get; set; }

        public string Subject { get; set; }

        public TBody Body { get; set; }

        public bool IsHtml { get; set; } = true;

        public string Theme { get; set; }

        public bool CanSend { get; set; } = true;
    }
}