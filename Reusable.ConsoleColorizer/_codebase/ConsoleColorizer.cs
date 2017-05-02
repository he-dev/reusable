using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reusable.Extensions;

namespace Reusable
{
    public class ConsoleColorizer
    {
        public static void Render(string xml)
        {
            Render(XElement.Parse(xml.NullIfEmpty() ?? throw new ArgumentNullException(nameof(xml))).Nodes());
            Console.ResetColor();
        }

        public static void RenderLine(string xml)
        {
            Render(XElement.Parse(xml.NullIfEmpty() ?? throw new ArgumentNullException(nameof(xml))).Nodes());
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void Render(IEnumerable<XNode> xNodes)
        {
            RenderInternal(xNodes ?? throw new ArgumentNullException(nameof(xNodes)));
        }

        private static void RenderInternal(IEnumerable<XNode> xNodes)
        {
            foreach (var xNode in xNodes)
            {
                switch (xNode)
                {
                    case XElement xElement: Render(xElement); break;
                    case XText xText: Render(xText); break;
                }
            }
        }

        private static void Render(XElement xElement)
        {
            using (var currentStyle = new Usingifier<ConsoleStyle>(() => ConsoleStyle.Current, style => style.Apply()))
            {
                currentStyle.Initialize();
                ConsoleStyle.Parse(xElement).Apply();
                RenderInternal(xElement.Nodes());
            }
        }

        private static void Render(XText xText)
        {
            Console.Write(xText.Value);
        }
    }
}
