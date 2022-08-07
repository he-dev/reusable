using Reusable.Marbles;
using Reusable.Prism;
using Reusable.Prism.Elements;
using static System.ConsoleColor;

namespace Reusable;

public static class PrismDemo
{
    private record Message(string Greeting, string Place, string Punctuation);
    
    public static void Start()
    {
        var message = new Template<Message>
        {
            ctx => new Text.Repeated.Whitespace(3),
            ctx => new Text(ctx.State.Greeting, new Style
            {
                Color = new Color(Foreground: Magenta),
                Padding = new Padding(Right: 1)
            }),
            ctx => new Text.Quoted.Square(ctx.State.Place , "Square"),
            ctx => new Text(ctx.State.Punctuation, new Style
            {
                Color = new Color(Foreground: DarkCyan),
            })
        };
        
        var css = new ConsoleStyleSheet
        {
            {
                "Square",
                new Style { Color = new Color(Foreground: DarkGray) },
                new Style { Color = new Color(Foreground: DarkMagenta), Width = 4},
                new Style { Color = new Color(Foreground: DarkGray) }
            }
        };

        message.Render(new Message("Hall…o", "console", "!"), css);
    }
}