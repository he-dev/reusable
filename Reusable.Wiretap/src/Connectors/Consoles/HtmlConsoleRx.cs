using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Data;
using Reusable.OmniLog.Services;

namespace Reusable.OmniLog.Connectors
{
    [PublicAPI]
    public class HtmlConsoleRx : IConnector
    {
        public static readonly ConsoleStyle DefaultStyle = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);

        private readonly object _syncLock = new object();

        public IConsoleStyle Style { get; set; } = DefaultStyle;

        public IConsoleTemplateBuilder<HtmlElement>? TemplateBuilder { get; set; }

        /// <summary>
        /// Renders the Html to the console. This method is thread-safe.
        /// </summary>
        public void Log(ILogEntry entry)
        {
            lock (_syncLock)
            {
                if (entry.TryGetProperty(Names.Properties.Message, out var property) && property.Value is IHtmlConsoleTemplateBuilder builder)
                {
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