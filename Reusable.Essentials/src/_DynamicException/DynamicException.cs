using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials;

[PublicAPI]
public abstract class DynamicException : Exception
{
    protected DynamicException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets the default dynamic exception factory. It uses an internal cache for each exception type.
    /// </summary>
    public static IDynamicExceptionFactory Factory => DynamicExceptionFactory.Default;

    public static Exception Create(DynamicExceptionName name, string? message = default, Exception? innerException = default)
    {
        return Factory.CreateDynamicException(name, message, innerException);
    }
    
    public static Exception Create<T>(string? message = default, Exception? innerException = default)
    {
        return Factory.CreateDynamicException(typeof(T).ToPrettyString(), message, innerException);
    }

    public static Exception Create<T>(T obj, string? message = default, Exception? innerException = default)
    {
        return Create<T>(message, innerException);
    }
}

public readonly struct DynamicExceptionName
{
    private readonly string _name;

    private DynamicExceptionName(string name)
    {
        _name =
            Regex.IsMatch(name, $"{nameof(Exception)}$", RegexOptions.IgnoreCase)
                ? name
                : $"{name}{nameof(Exception)}";
    }

    public override string ToString() => _name;

    public static implicit operator DynamicExceptionName(string name) => new(name);

    public static implicit operator string(DynamicExceptionName name) => name.ToString();
}