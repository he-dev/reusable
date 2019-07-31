using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.MarkupBuilder.Html;

namespace Reusable.OmniLog.Console
{
    public interface IConsoleRenderer
    {
        //void Render([NotNull] string template);
        void Render([NotNull] HtmlElement template);
    }

    public class ConsoleRenderer : IConsoleRenderer
    {
        private readonly object _syncLock = new object();

        /// <summary>
        /// Renders the Html to the console. This method is thread-safe.
        /// </summary>
        public void Render(HtmlElement template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            lock (_syncLock)
            {
                using (ConsoleStyle.From(template).Apply())
                {
                    Render(template.AsEnumerable());
                }

                var isParagraph = template.Name.Equals("p");
                if (isParagraph)
                {
                    System.Console.WriteLine();
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
            System.Console.Write(text);
        }
    }

//    public class ConsoleRenderer : IConsoleRenderer
//    {
//        private readonly object _syncLock = new object();
//
//        /// <summary>
//        /// Renders the Html to the console. This method is thread-safe.
//        /// </summary>
//        public void Render(string template)
//        {
//            if (template == null) throw new ArgumentNullException(nameof(template));
//
//            lock (_syncLock)
//            {
//                var xTemplate = XElement.Parse(template, LoadOptions.PreserveWhitespace);
//
//                Render(xTemplate.Nodes());
//
//                var isParagraph = xTemplate.Name.LocalName.Equals("p");
//                if (isParagraph)
//                {
//                    System.Console.WriteLine();
//                }
//            }
//        }
//
//        private static void Render(IEnumerable<XNode> xNodes)
//        {
//            foreach (var xNode in xNodes)
//            {
//                switch (xNode)
//                {
//                    case XElement xElement:
//                        Render(xElement);
//                        break;
//                    case XText xText:
//                        Render(xText);
//                        break;
//                }
//            }
//        }
//
//        private static void Render(XElement xElement)
//        {
//            using (ConsoleStyle.Parse(xElement).Apply())
//            {
//                Render(xElement.Nodes());
//            }
//        }
//
//        private static void Render(XText xText)
//        {
//            System.Console.Write(xText.Value);
//        }
//    }
}