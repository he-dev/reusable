using System;
using Reusable.Marbles;

namespace Reusable.Prism.Elements;

public interface IElement
{
    void Render(ConsoleStyleSheet css);
}

public record Text(string Value, Style? Style = default) : IElement
{
    public Text(char c, Style? style = default) : this(c.ToString()) { }

    public void Render(ConsoleStyleSheet css)
    {
        Console.Write(new string(' ', Style?.Margin?.Left ?? 0));

        using (Style?.Color?.Apply() ?? Disposable.Empty)
        {
            Console.Write(new string(' ', Style?.Padding?.Left ?? 0));
            Console.Write(Style is { Width: { } } ? Value[..(Style.Width.Value - 1)] + '…' : Value);
            Console.Write(new string(' ', Style?.Padding?.Right ?? 0));
        }

        Console.Write(new string(' ', Style?.Margin?.Right ?? 0));
    }

    public record Repeated : Text
    {
        public Repeated(char c, int count) : base(new string(c, count)) { }

        public record Whitespace : Repeated
        {
            public Whitespace(int count) : base(' ', count) { }
        }
    }

    public record Quoted(char Left, string Text, char Right, string Style) : IElement
    {
        public void Render(ConsoleStyleSheet css)
        {
            new Text(Left, css[$"{Style}:{nameof(Left)}"]).Render(css);
            new Text(Text, css[$"{Style}"]).Render(css);
            new Text(Right, css[$"{Style}:{nameof(Right)}"]).Render(css);
        }

        public record Square(string Text, string Style) : Quoted('[', Text, ']', Style);
    };
}

public record LineBreak : IElement
{
    public void Render(ConsoleStyleSheet css)
    {
        Console.WriteLine();
    }
}