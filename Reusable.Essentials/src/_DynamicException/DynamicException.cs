using System;

namespace Reusable.Essentials;

public abstract class DynamicException : Exception
{
    protected DynamicException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets the default dynamic exception factory. It uses an internal cache for each exception type.
    /// </summary>
    public static IDynamicExceptionFactory Factory => DynamicExceptionFactory.Default;

    public static Exception Create(ExceptionName name, string? message = default, Exception? innerException = default)
    {
        return Factory.CreateDynamicException(name, message, innerException);
    }
}