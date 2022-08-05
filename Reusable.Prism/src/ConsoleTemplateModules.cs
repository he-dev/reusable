using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Marbles;

namespace Reusable.Prism;

public record ConsolePadding(int Left, int Right);

public interface ISpan
{
    void Render<T>(ConsoleStyleSheet css, T parameter);
}

public record TextSpan(string Text) : ISpan
{
    public ConsoleStyle? Style { get; set; }

    public ConsolePadding? Padding { get; set; }

    public virtual void Render<T>(ConsoleStyleSheet css, T parameter)
    {
        using var style = Style?.Apply() ?? Disposable.Empty;
        Console.Write(new string(' ', Padding?.Left ?? 0));
        Console.Write(Text);
        Console.Write(new string(' ', Padding?.Right ?? 0));
    }

    public record Repeated : TextSpan
    {
        public Repeated(char c, int count) : base(new string(c, count)) { }

        public record Whitespace : Repeated
        {
            public Whitespace(int count) : base(' ', count) { }
        }
    }
}

public record QuoteSpan : ISpan
{
    private Quote Quote { get; }

    private ISpan Span { get; }

    public QuoteSpan(Quote quote, ISpan span)
    {
        Quote = quote;
        Span = span;
    }

    public ConsoleStyle? Style { get; set; }

    public ConsolePadding? Padding { get; set; }

    public void Render<T>(ConsoleStyleSheet css, T parameter)
    {
        using (Style?.Apply() ?? Disposable.Empty)
        {
            Console.Write(new string(' ', Padding?.Left ?? 0));
            Console.Write(Quote.Left);
        }

        Span.Render(css, parameter);

        using (Style?.Apply() ?? Disposable.Empty)
        {
            Console.Write(Quote.Right);
            Console.Write(new string(' ', Padding?.Right ?? 0));
        }
    }
}

public delegate ISpan ConsoleSpanFactory<in T>(ConsoleStyleSheet css, T parameter);

public delegate ConsoleStyle? ConsoleParagraphFactory<in T>(ConsoleStyleSheet css, T parameter);

public class ConsoleTemplate<T>
{
    private ConsoleParagraphFactory<T>? CreateParagraph { get; }

    private IEnumerable<ConsoleSpanFactory<T>> SpanFactories { get; }

    public ConsoleTemplate(ConsoleParagraphFactory<T>? createParagraph, IEnumerable<ConsoleSpanFactory<T>> spanFactories)
    {
        CreateParagraph = createParagraph;
        SpanFactories = spanFactories;
    }

    public void Render(ConsoleStyleSheet css, T parameter)
    {
        using var paragraph = CreateParagraph.Let(f => f?.Invoke(css, parameter)?.Apply() ?? Disposable.Empty);

        if (paragraph is ConsoleStyle.Restore)
        {
            Console.WriteLine();
        }

        foreach (var createSpan in SpanFactories)
        {
            createSpan(css, parameter).Render(css, paragraph);
        }
    }
}

public static class Test
{
    private record Message(string Text);

    public static void Example()
    {
        var message =
            new ConsoleTemplateBuilder<Message>()
                .Append((_, _) => new TextSpan.Repeated.Whitespace(3))
                .Append((_, p) => new TextSpan(p.Text)).Build();

        message.Render(new ConsoleStyleSheet(ConsoleStyle.Current), new Message("Hallo!"));
    }
}

public class ConsoleTemplateBuilder<T>
{
    private List<ConsoleSpanFactory<T>> SpanFactories { get; } = new();

    public ConsoleTemplateBuilder<T> Append(ConsoleSpanFactory<T> createSpan) => this.Also(x => x.SpanFactories.Add(createSpan));

    public ConsoleTemplate<T> Build(ConsoleParagraphFactory<T>? createParagraph = default) => new(createParagraph, SpanFactories);
}