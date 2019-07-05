using Reusable.Data;

namespace Reusable.IOnymous
{
    public abstract class FileProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "file";

        protected FileProvider(IImmutableContainer metadata)
            : base(metadata ?? ImmutableContainer.Empty.SetScheme(DefaultScheme)) { }
    }
}