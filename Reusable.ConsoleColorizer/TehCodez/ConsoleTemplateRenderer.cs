using System;
using System.Collections.Generic;
using System.Xml.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.ConsoleColorizer
{
    public interface IConsoleTemplateRenderer
    {
        void Render([NotNull] string template);
    }
    
    public class ConsoleTemplateRenderer : IConsoleTemplateRenderer
    {
        private readonly object _syncLock = new object();

        /// <summary>
        /// Renders the Html to the console. This method is thread-safe.
        /// </summary>
        public void Render(string template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            lock (_syncLock)
            {
                var xTemplate = XElement.Parse(template, LoadOptions.PreserveWhitespace);

                Render(xTemplate.Nodes());

                var isParagraph = xTemplate.Name.LocalName.Equals("p");
                if (isParagraph)
                {
                    Console.WriteLine();
                }
            }
        }

        private static void Render(IEnumerable<XNode> xNodes)
        {
            foreach (var xNode in xNodes)
            {
                switch (xNode)
                {
                    case XElement xElement:
                        Render(xElement);
                        break;
                    case XText xText:
                        Render(xText);
                        break;
                }
            }
        }

        private static void Render(XElement xElement)
        {
            using (ConsoleStyle.Parse(xElement).Apply())
            {
                Render(xElement.Nodes());
            }
        }

        private static void Render(XText xText)
        {
            Console.Write(xText.Value);
        }
    }
}