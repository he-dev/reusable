using System;
using System.Xml.Linq;

namespace Reusable
{
    public class ConsoleStyle
    {
        public ConsoleStyle(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public static ConsoleStyle Current => new ConsoleStyle(Console.ForegroundColor, Console.BackgroundColor);

        public ConsoleColor ForegroundColor { get; }

        public ConsoleColor BackgroundColor { get; }

        public static ConsoleStyle Parse(XElement xElement)
        {
            return new ConsoleStyle(
                Enum.TryParse(xElement.Attribute("color")?.Value, true, out ConsoleColor foregroundColor) ? foregroundColor : Console.ForegroundColor,
                Enum.TryParse(xElement.Attribute("background-color")?.Value, true, out ConsoleColor backgroundColor) ? backgroundColor : Console.BackgroundColor);
        }

        public void Apply()
        {
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
        }
    }
}