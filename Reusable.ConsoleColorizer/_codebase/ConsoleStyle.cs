using System;

namespace Reusable
{
    public struct ConsoleStyle
    {
        public ConsoleStyle(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            : this()
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public ConsoleStyle(string foregroundColorName, string backgroundColorName)
        {
            ConsoleColor foregroundColor;
            ForegroundColor = Enum.TryParse(foregroundColorName, true, out foregroundColor)
                ? foregroundColor
                : Console.ForegroundColor;

            ConsoleColor backgroundColor;
            BackgroundColor = Enum.TryParse(backgroundColorName, true, out backgroundColor)
                ? backgroundColor
                : Console.BackgroundColor;
        }

        public ConsoleColor ForegroundColor { get; }

        public ConsoleColor BackgroundColor { get; }

        public static ConsoleStyle Current => new ConsoleStyle(Console.ForegroundColor, Console.BackgroundColor);

        public void Apply()
        {
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
        }
    }
}