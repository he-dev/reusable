using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Marbles.FormatProviders;

public abstract class CustomFormatProvider : IFormatProvider
{
    private readonly ICustomFormatter _formatter;

    protected CustomFormatProvider(ICustomFormatter formatter) => _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

    public virtual object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? _formatter : null;
}

public class CompositeFormatProvider : IFormatProvider, IEnumerable<IFormatProvider>
{
    private readonly ISet<IFormatProvider> _formatProviders;

    private readonly ICustomFormatter _formatter;

    public CompositeFormatProvider(params IFormatProvider[] formatProviders)
    {
        _formatProviders =
            new HashSet<IFormatProvider>(
                formatProviders
                    .Select(x => x is IEnumerable<IFormatProvider> enumerable ? enumerable : new[] { x })
                    .SelectMany(x => x)
            );
        _formatter =
            new Formatter(
                _formatProviders
                    .Select(formatProvider => formatProvider.GetFormat(typeof(ICustomFormatter)))
                    .Cast<ICustomFormatter>()
            );
    }

    public object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? _formatter : null;

    public void Add(Type formatProvider)
    {
        if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));

        _formatProviders.Add(
            typeof(IFormatProvider).IsAssignableFrom(formatProvider)
                ? (IFormatProvider)Activator.CreateInstance(formatProvider)
                : throw new ArgumentException($"'{formatProvider}' must be of type '{nameof(IFormatProvider)}'")
        );
    }

    public void Add(IFormatProvider formatProvider)
    {
        if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));

        _formatProviders.Add(formatProvider);
    }

    public IEnumerator<IFormatProvider> GetEnumerator() => _formatProviders.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class Formatter : ICustomFormatter
    {
        private readonly IEnumerable<ICustomFormatter> _formatters;

        public Formatter(IEnumerable<ICustomFormatter> formatters) => _formatters = formatters;

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is null || format is null) { return null; }

            var formats = format.Split('|');

            var result =
                _formatters
                    .Select(formatter => formatter.Format(formats.FirstOrDefault(), arg, formatProvider))
                    .FirstOrDefault(Conditional.IsNotNull);

            foreach (var x in formats.Skip(1))
            {
                result =
                    _formatters
                        .Select(formatter => formatter.Format(x, result, formatProvider))
                        .FirstOrDefault(Conditional.IsNotNull);
                if (result is null) { break; }
            }

            return result;
        }
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class FormatProviderAttribute : Attribute
{
    public FormatProviderAttribute(Type type)
    {
        FormatProvider =
            typeof(IFormatProvider).IsAssignableFrom(type)
                ? (IFormatProvider)Activator.CreateInstance(type)
                : throw new ArgumentException($"'{nameof(type)}' must implement the '{typeof(IFormatProvider).FullName}'.", nameof(type));
    }

    public string Format { get; set; }

    public IFormatProvider FormatProvider { get; }
}