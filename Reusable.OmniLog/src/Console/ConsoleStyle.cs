using System;
using System.Linq;
using System.Xml.Linq;
using Reusable.MarkupBuilder.Html;

namespace Reusable.OmniLog.Console
{
    public interface IConsoleStyle
    {
        ConsoleColor ForegroundColor { get; }

        ConsoleColor BackgroundColor { get; }

        IDisposable Apply();
    }

    public readonly struct ConsoleStyle : IConsoleStyle
    {
        // language=regexp
        //private const string StylePattern = "background-color: (?<BackgroundColor>([a-z]+))|color: (?<ForegroundColor>([a-z]+))";

        //private static readonly AsyncLocal<ConsoleStyle> _current = new AsyncLocal<ConsoleStyle>();

        public ConsoleStyle(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public static ConsoleStyle Current => new ConsoleStyle(System.Console.BackgroundColor, System.Console.ForegroundColor);

        public ConsoleColor ForegroundColor { get; }

        public ConsoleColor BackgroundColor { get; }

        public static ConsoleStyle Parse(XElement xElement)
        {
            var style = xElement.Attribute("style")?.Value;
            if (style is null)
            {
                return Current;
            }

            // https://regex101.com/r/V9XIgD/2

            var declarations = style.ToDeclarations().ToDictionary(x => x.property, x => x.value);

            //            var styleMatch = Regex.Match(style, StylePattern, RegexOptions.ExplicitCapture);
            //            if (!styleMatch.Success)
            //            {
            //                return Current;
            //            }                       

            var foregroundColor = string.Empty;
            var backgroundColor = string.Empty;

            declarations.TryGetValue("color", out foregroundColor);
            declarations.TryGetValue("background-color", out backgroundColor);

            return new ConsoleStyle(Enum.TryParse(backgroundColor, true, out ConsoleColor consoleBackgroundColor) ? consoleBackgroundColor : System.Console.BackgroundColor, Enum.TryParse(foregroundColor, true, out ConsoleColor consoleForegroundColor) ? consoleForegroundColor : System.Console.ForegroundColor);
        }
        
        public static ConsoleStyle From(HtmlElement htmlElement)
        {
            if(!htmlElement.Attributes.TryGetValue("style", out var style))
            {
                return Current;
            }

            // https://regex101.com/r/V9XIgD/2

            var declarations = style.ToDeclarations().ToDictionary(x => x.property, x => x.value);

            //            var styleMatch = Regex.Match(style, StylePattern, RegexOptions.ExplicitCapture);
            //            if (!styleMatch.Success)
            //            {
            //                return Current;
            //            }                       

            var foregroundColor = string.Empty;
            var backgroundColor = string.Empty;

            declarations.TryGetValue("color", out foregroundColor);
            declarations.TryGetValue("background-color", out backgroundColor);

            return new ConsoleStyle(Enum.TryParse(backgroundColor, true, out ConsoleColor consoleBackgroundColor) ? consoleBackgroundColor : System.Console.BackgroundColor, Enum.TryParse(foregroundColor, true, out ConsoleColor consoleForegroundColor) ? consoleForegroundColor : System.Console.ForegroundColor);
        }

        public IDisposable Apply()
        {
            var restoreStyle = Current;
            System.Console.ForegroundColor = ForegroundColor;
            System.Console.BackgroundColor = BackgroundColor;

            return Disposable.Create(() =>
            {
                System.Console.ForegroundColor = restoreStyle.ForegroundColor;
                System.Console.BackgroundColor = restoreStyle.BackgroundColor;
            });
        }
    }
}