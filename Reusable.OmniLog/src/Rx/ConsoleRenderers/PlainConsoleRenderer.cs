using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    public class PlainConsoleRenderer : IConsoleRenderer
    {
        public string Template { get; set; } = "{Message}";

        public virtual void Render(LogEntry logEntry)
        {
            Console.WriteLine(Template.Format(logEntry));
        }
    }
}