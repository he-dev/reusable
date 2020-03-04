using System;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    [PublicAPI]
    public class PlainConsoleRx : ILogRx
    {
        public string Template { get; set; } = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Logger}: {Message}";

        public IFormatProvider FormatProvider { get; set; } = new CompositeFormatProvider
        {
            new CaseFormatProvider(),
            new PunctuationFormatProvider(),
            new TypeFormatProvider()
        };

        public virtual void Log(ILogEntry entry)
        {
            Console.WriteLine(Template.Format((string name, out object? value) =>
            {
                if (entry.TryGetProperty(name, out var property))
                {
                    value = property.Value;
                    return true;
                }
                else
                {
                    value = default!;
                    return false;
                }
            }, FormatProvider));
        }
    }
}