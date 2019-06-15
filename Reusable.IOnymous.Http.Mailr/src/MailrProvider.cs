using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Keynetic;

namespace Reusable.IOnymous
{
    public class MailrProvider : HttpProvider
    {
        public const string Name = "Mailr";

        public MailrProvider([NotNull] string baseUri, IImmutableSession metadata = default)
            : base(baseUri, metadata.ThisOrEmpty().SetItem(From<IProviderMeta>.Select(x => x.ProviderName), Name)) { }
    }
}