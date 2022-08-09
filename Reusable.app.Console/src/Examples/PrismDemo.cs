using Reusable.Marbles;
using Reusable.Prism;
using static System.ConsoleColor;

namespace Reusable;

public static class PrismDemo
{
    private record Message(string Greeting, string Place, string Punctuation);

    public static void Start()
    {
        var message =
            Template
                .For<Message>()
                .LineBreak()
                .Whitespace(3)
                .Text(x => x.Greeting, x => nameof(x.Greeting))
                .Quote('[', x => x.Place, ']', textStyle: x => nameof(x.Place), quoteStyle: _ => "square")
                .Repeat(_ => '-', 3)
                .Text(x => x.Punctuation, x => nameof(x.Punctuation))
                .LineBreak();

        var css = new StyleCollection
        {
            { "greeting", new Style { Color = new Color(Foreground: Magenta), Padding = new Padding(Right: 1) } },
            { "place", new Style { Color = new Color(Foreground: DarkMagenta), Width = 4 } },
            { "punctuation", new Style { Color = new Color(Foreground: DarkCyan) } },
            { "square", new Style { Color = new Color(Foreground: DarkGray) } },
        };

        message.Render(new Message("Hall…o", "console", "!"), css);
    }
}