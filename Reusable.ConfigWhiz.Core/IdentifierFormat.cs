namespace Reusable.ConfigWhiz
{
    public class IdentifierFormat
    {
        private IdentifierFormat(string format) => Format = format;
        public string Format { get; }
        //public static readonly IdentifierFormat ShortWeak = new IdentifierFormat(".a");
        public static readonly IdentifierFormat ShortWeak = new IdentifierFormat(".a");
        public static readonly IdentifierFormat ShortStrong = new IdentifierFormat(".a[]");
        public static readonly IdentifierFormat FullWeak = new IdentifierFormat("a.b");
        public static readonly IdentifierFormat FullStrong = new IdentifierFormat("a.b[]");
        public static implicit operator string(IdentifierFormat format) => format.Format;
    }
}