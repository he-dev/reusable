using System;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    [PublicAPI]
    public class PlainConsoleRenderer : IConsoleRenderer
    {
        public string Template { get; set; } = @"[{Timestamp:HH:mm:ss:fff}] [{Level:u}] {Logger}: {Message}";

        public IFormatProvider FormatProvider { get; set; } = new CompositeFormatProvider
        {
            new CaseFormatProvider(),
            new PunctuationFormatProvider(),
            new TypeFormatProvider()
        };

        public virtual void Render(LogEntry logEntry)
        {
            Console.WriteLine(Template.Format((string name, out object value) => logEntry.TryGetItem(name, default, out value), FormatProvider));
        }
    }
}