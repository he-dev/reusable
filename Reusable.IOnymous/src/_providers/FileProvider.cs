namespace Reusable.IOnymous
{
    public abstract class FileProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "file";

        protected FileProvider(Metadata metadata)
            : base(new SoftString[] { DefaultScheme }, metadata) { }
    }
}