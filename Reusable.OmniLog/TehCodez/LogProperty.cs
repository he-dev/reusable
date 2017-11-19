namespace Reusable.OmniLog
{
    // This class provides names for most common log properties.    
    public static class LogProperty
    {
        public static readonly SoftString Scope = nameof(Scope);
        public static readonly SoftString Category = nameof(Category);
        public static readonly SoftString LogLevel = nameof(LogLevel);
        public static readonly SoftString Message = nameof(Message);
        public static readonly SoftString Exception = nameof(Exception);
        public static readonly SoftString Elapsed = nameof(Elapsed);
        public static readonly SoftString Timestamp = nameof(Timestamp);
        public static readonly SoftString CallerMemberName = nameof(CallerMemberName);
        public static readonly SoftString CallerLineNumber = nameof(CallerLineNumber);
        public static readonly SoftString CallerFilePath = nameof(CallerFilePath);

        // This field can be used to remove a property from log.
        public static readonly object Unset = new object();
    }
}