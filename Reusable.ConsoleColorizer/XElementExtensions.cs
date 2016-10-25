using System.Xml.Linq;

namespace Reusable
{
    internal static class XElementExtensions
    {
        public static ConsoleStyle ToConsoleStyle(this XElement xElement, string foregroundAttributeName = "fg", string backgroundAttributeName = "bg")
        {
            if (xElement == null) { return ConsoleStyle.Current; }

            return new ConsoleStyle(
                xElement.Attribute(foregroundAttributeName)?.Value,
                xElement.Attribute(backgroundAttributeName)?.Value);
        }
    }
}