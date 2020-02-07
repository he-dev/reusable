using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Helpers;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Rx.Consoles
{
    [PublicAPI]
    public class HtmlConsoleRx : ILogRx
    {
        public static readonly ConsoleStyle DefaultStyle = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);

        private readonly object _syncLock = new object();

        public IConsoleStyle Style { get; set; } = DefaultStyle;

        public IConsoleTemplateBuilder<HtmlElement>? TemplateBuilder { get; set; }

        /// <summary>
        /// Renders the Html to the console. This method is thread-safe.
        /// </summary>
        public void Log(LogEntry entry)
        {
            lock (_syncLock)
            {
                var builder = entry.TryGetProperty(LogEntry.Names.Message, m => m.ProcessWith<EchoNode>().LogWith(this), out var property) switch
                {
                    true => property.Value as IHtmlConsoleTemplateBuilder,
                    false => TemplateBuilder
                };

                if (builder is null)
                {
                    return;
                }

                var template = builder.Build(entry);

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