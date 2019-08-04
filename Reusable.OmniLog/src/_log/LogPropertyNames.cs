namespace Reusable.OmniLog
{
    /// <summary>
    /// This class provides default property names.
    /// </summary>
    public static class LogPropertyNames
    {
        public static readonly SoftString Logger = nameof(Logger);
        public static readonly SoftString Category = nameof(Category);
        public static readonly SoftString Level = nameof(Level);
        public static readonly SoftString Message = nameof(Message);
        public static readonly SoftString Exception = nameof(Exception);
        public static readonly SoftString Elapsed = nameof(Elapsed);
        public static readonly SoftString Timestamp = nameof(Timestamp);
        public static readonly SoftString CallerMemberName = nameof(CallerMemberName);
        public static readonly SoftString CallerLineNumber = nameof(CallerLineNumber);
        public static readonly SoftString CallerFilePath = nameof(CallerFilePath);
        public static readonly SoftString OverridesTransaction = nameof(OverridesTransaction);

        //public static readonly SoftString Scope = nameof(Scope);
        //public static readonly SoftString CorrelationId = nameof(CorrelationId);
        //public static readonly SoftString Context = nameof(Context);

        // This field can be used to remove a property from log.
        public static readonly object Unset = new object();
    }
}