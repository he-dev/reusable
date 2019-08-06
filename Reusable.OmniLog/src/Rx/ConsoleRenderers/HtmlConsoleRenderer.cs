using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    public class HtmlConsoleRenderer : IConsoleRenderer
    {
        public static readonly ConsoleStyle DefaultStyle = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);

        private readonly object _syncLock = new object();

        public IConsoleStyle Style { get; set; } = DefaultStyle;

        [CanBeNull]
        public HtmlConsoleTemplateBuilder TemplateBuilder { get; set; }

        /// <summary>
        /// Renders the Html to the console. This method is thread-safe.
        /// </summary>
        public void Render(LogEntry logEntry)
        {
            lock (_syncLock)
            {
                if (logEntry.GetItemOrDefault(LogEntry.BasicPropertyNames.Message, nameof(HtmlConsoleTemplateBuilder), TemplateBuilder) is var builder && builder is null)
                {
                    return;
                }

                var template = builder.Build(logEntry);

                using (ConsoleStyle.From(template).Apply())
                {
                    Render(template.AsEnumerable());
                }

                if (template.Name.Equals(nameof(MarkupBuilder.Html.HtmlElementExtensions.p)))
                {
                    Console.WriteLine();
                }
            }
        }

        private static void Render(IEnumerable<object> values)
        {
            foreach (var value in values)
            {
                switch (value)
                {
                    case HtmlElement htmlElement:
                        RenderSingle(htmlElement);
                        break;
                    case string text:
                        Render(text);
                        break;
                }
            }
        }

        private static void RenderSingle(HtmlElement htmlElement)
        {
            using (ConsoleStyle.From(htmlElement).Apply())
            {
                Render(htmlElement.AsEnumerable());
            }
        }

        private static void Render(string text)
        {
            Console.Write(text);
        }
    }
}