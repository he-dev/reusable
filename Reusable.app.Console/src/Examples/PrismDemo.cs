using System;
using Reusable.Marbles;
using Reusable.Prism;

namespace Reusable;

public static class PrismDemo
{
    private record Message(string Greeting, string Place, string Punctuation);

    public static void Start()
    {
        var message =
            new ConsoleTemplateBuilder<Message>()
                .Append((_, _) => new TextSpan.Repeated.Whitespace(3))
                .Append((_, p) => new TextSpan(p.Greeting)
                {
                    Style = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Magenta),
                    Padding = new ConsolePadding(1, 0)
                })
                .Append((_, p) => new TextSpan(p.Place)
                {
                    Style = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkMagenta),
                    Padding = new ConsolePadding(1, 0)
                })
                .Append((_, p) => new TextSpan(p.Punctuation)
                {
                    Style = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkCyan),
                    //Padding = new ConsolePadding(1, 0)
                })
                .Build();

        message.Render(new ConsoleStyleSheet(ConsoleStyle.Current), new Message("Hallo", "console", "!"));
    }
}