using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Reusable
{
    internal class ConsoleColorizer
    {
        public static void Render(string xml)
        {
            Render(XElement.Parse(xml).Nodes());
        }

        public static void Render(IEnumerable<XNode> xNodes)
        {
            RenderInternal(xNodes);
            Console.ResetColor();
        }

        private static void RenderInternal(IEnumerable<XNode> xNodes)
        {
            foreach (var xNode in xNodes)
            {
                new Func<bool>(() => Render(xNode as XElement) || Render(xNode))();
            }
        }

        private static bool Render(XElement xElement)
        {
            if (xElement == null)
            {
                return false;
            }

            var savedConsoleStyle = ConsoleStyle.Current;
            xElement.ToConsoleStyle().Apply();
            RenderInternal(xElement.Nodes());
            savedConsoleStyle.Apply();
            return true;
        }

        private static bool Render(XNode xNode)
        {
            var xText = xNode as XText;
            if (xText == null)
            {
                return false;
            }
            Console.Write(xText.Value);
            return true;
        }
    }
}
