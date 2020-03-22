using System;
using System.Collections.Generic;
using System.Linq.Custom;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Services;

// ReSharper disable once CheckNamespace
namespace Reusable.Commander.ConsoleTemplates
{
    using static ConsoleColor;

//    public class Prompt : ConsoleTemplateBuilder
//    {
//        public DateTime Timestamp => DateTime.Now;
//
//        public override HtmlElement Build(ILog log) =>
//            HtmlElement
//                .Builder
//                .span(span => span.text($"[{Timestamp:yyyy-MM-dd HH:mm:ss}]>"));
//    }

    public class Indent : IHtmlConsoleTemplateBuilder
    {
        private readonly int _depth;

        public Indent(int depth)
        {
            _depth = depth;
        }

        public int Width { get; set; } = 1;

        public HtmlElement Build(ILogEntry logEntry) =>
            HtmlElement
                .Builder
                .span(x => x.text(new string(' ', Width * _depth)));
    }

    namespace Help
    {
        public class TableRow : IHtmlConsoleTemplateBuilder
        {
            public IEnumerable<string> Cells { get; set; } = default!;

            public HtmlElement Build(ILogEntry logEntry) =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .text(Cells.Join(string.Empty)));
        }
    }


    public class Error : IHtmlConsoleTemplateBuilder
    {
        public string Text { get; set; } = default!;

        public HtmlElement Build(ILogEntry logEntry) =>
            HtmlElement
                .Builder
                .span(x => x
                    .text(Text)).color(Red);
    }
    
    public class Info : IHtmlConsoleTemplateBuilder
    {
        public string Text { get; set; } = default!;

        public HtmlElement Build(ILogEntry logEntry) =>
            HtmlElement
                .Builder
                .span(x => x
                    .text(Text));
    }
}