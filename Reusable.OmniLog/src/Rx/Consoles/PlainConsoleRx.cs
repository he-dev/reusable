using System;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Rx.Consoles
{
    [PublicAPI]
    public class PlainConsoleRx : ILogRx
    {
        public string Template { get; set; } = @"[{Timestamp:HH:mm:ss:fff}] [{Level:u}] {Logger}: {Message}";

        public IFormatProvider FormatProvider { get; set; } = new CompositeFormatProvider
        {
            new CaseFormatProvider(),
            new PunctuationFormatProvider(),
            new TypeFormatProvider()
        };

        public virtual void Log(LogEntry entry)
        {
            Console.WriteLine(Template.Format((string name, out object? value) =>
            {
                if (entry.TryGetProperty(name!, m => m.ProcessWith<EchoNode>(), out var property))
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