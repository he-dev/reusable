using System;
using System.Linq;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    public class PlainConsoleRenderer : IConsoleRenderer
    {
        public string Template { get; set; } = @"[{Timestamp:HH:mm:ss:fff}] [{Logger:u}] {Message}";

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