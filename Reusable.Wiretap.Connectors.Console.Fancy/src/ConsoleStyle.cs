using System;
using System.Linq;
using System.Xml.Linq;
using Reusable.Essentials;
using Reusable.Fluorite;
using Reusable.Fluorite.Html;

namespace Reusable.Wiretap.Connectors.Data
{
    public interface IConsoleStyle
    {
        ConsoleColor ForegroundColor { get; }

        ConsoleColor BackgroundColor { get; }

        IDisposable Apply();
    }

    public readonly struct ConsoleStyle : IConsoleStyle
    {
        public ConsoleStyle(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public static ConsoleStyle Current => new ConsoleStyle(Console.BackgroundColor, Console.ForegroundColor);

        public ConsoleColor ForegroundColor { get; }

        public ConsoleColor BackgroundColor { get; }

        public static ConsoleStyle From(XElement xElement)
        {
            var style = xElement.Attribute("style")?.Value;
            if (style is null)
            {
                return Current;
            }

            // https://regex101.com/r/V9XIgD/2

            var declarations = style.ToDeclarations().ToDictionary(x => x.property, x => x.value);

            declarations.TryGetValue("color", out var foregroundColor);
            declarations.TryGetValue("background-color", out var backgroundColor);

            return Parse(backgroundColor, foregroundColor);
        }

        public static ConsoleStyle Parse(string backgroundColor, string foregroundColor)
        {
            return new ConsoleStyle
            (
                Enum.TryParse(backgroundColor, true, out ConsoleColor consoleBackgroundColor) ? consoleBackgroundColor : Console.BackgroundColor,
                Enum.TryParse(foregroundColor, true, out ConsoleColor consoleForegroundColor) ? consoleForegroundColor : Console.ForegroundColor
            );
        }


        public static ConsoleStyle From(HtmlElement htmlElement)
        {
            if (!htmlElement.Attributes.TryGetValue("style", out var style))
            {
                return Current;
            }

            var declarations = style.ToDeclarations().ToDictionary(x => x.property, x => x.value);

            declarations.TryGetValue("color", out var foregroundColor);
            declarations.TryGetValue("background-color", out var backgroundColor);

            return Parse(backgroundColor, foregroundColor);
        }

        public IDisposable Apply()
        {
            // Backup the current style.
            var styleCopy = Current;

            // Apply a new stile.
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;

            // Restore the previous style.
            return Disposable.Create(() =>
            {
                Console.ForegroundColor = styleCopy.ForegroundColor;
                Console.BackgroundColor = styleCopy.BackgroundColor;
            });
        }
    }
}