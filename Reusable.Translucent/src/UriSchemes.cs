namespace Reusable.Translucent
{
    public static class UriSchemes
    {
        public static class Known
        {
            public static readonly string File = nameof(File).ToLower();
            public static readonly string Http = nameof(Http).ToLower();
            public static readonly string Https = nameof(Https).ToLower();
            public static readonly string MailTo = nameof(MailTo).ToLower();
        }

        public static class Custom
        {
            // ReSharper disable once InconsistentNaming - This is the name of the framework and this spelling is correct.
            public static readonly string IOnymous = nameof(IOnymous).ToLower();
        }
    }
}