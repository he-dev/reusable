using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class MailrProvider : HttpProvider
    {
        public static SoftString Tag = CreateTag("Mailr");

        public MailrProvider([NotNull] string baseUri, IImmutableContainer metadata = default)
            : base(baseUri, metadata.ThisOrEmpty().SetName(Tag)) { }
    }
}