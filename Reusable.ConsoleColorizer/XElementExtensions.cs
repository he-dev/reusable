using System.Xml.Linq;

namespace Reusable
{
    internal static class XElementExtensions
    {
        public static ConsoleStyle ToConsoleStyle(this XElement xElement, string foregroundAttributeName = "fg", string backgroundAttributeName = "bg")
        {
            return xElement == null ? ConsoleStyle.Current : new ConsoleStyle(
                xElement.Attribute(foregroundAttributeName)?.Value,
                xElement.Attribute(backgroundAttributeName)?.Value);
        }
    }
}