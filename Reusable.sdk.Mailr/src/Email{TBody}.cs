using JetBrains.Annotations;

namespace Reusable.sdk.Mailr
{
    [PublicAPI]
    public class Email<TBody>
    {
        public string To { get; set; }

        public string Subject { get; set; }

        public TBody Body { get; set; }

        public bool IsHtml { get; set; } = true;

        public string Theme { get; set; }

        public bool CanSend { get; set; } = true;
    }
}