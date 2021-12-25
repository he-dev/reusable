using System;
using System.Linq.Expressions;

namespace Reusable.Essentials;

public static class FingerprintBuilderExtensions
{
    public static FingerprintBuilder<T> Add<T, TInput>(this FingerprintBuilder<T> builder, Expression<Func<T, TInput>> getInputExpression)
    {
        return builder.Add(getInputExpression, _ => _);
    }

    public static FingerprintBuilder<T> Add<T>(this FingerprintBuilder<T> builder, Expression<Func<T, string>> getInputExpression, StringOptions options = StringOptions.None)
    {
        return builder.Add(getInputExpression, input => Format(input, options));
    }

    private static string Format(string input, StringOptions options)
    {
        if (options.HasFlag(StringOptions.IgnoreCase))
        {
            input = input.ToUpperInvariant();
        }

        if (options.HasFlag(StringOptions.IgnoreWhitespace))
        {
            input = input.Trim();
        }

        return input;
    }
}
    
    
[Flags]
public enum StringOptions
{
    None = 0,
    IgnoreCase = 1 << 0,
    IgnoreWhitespace = 1 << 1,
}