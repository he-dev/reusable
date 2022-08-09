using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Marbles;

namespace Reusable.Prism;

public interface ITemplateContext<out T>
{
    StyleCollection Styles { get; }
    
    T State { get; }
}

public record TemplateContext<T>(T State, StyleCollection Styles) : ITemplateContext<T>;

public delegate void RenderFunc<in T>(ITemplateContext<T> context);

public class Template<T> : IEnumerable<RenderFunc<T>>
{
    private List<RenderFunc<T>> Actions { get; } = new();

    public IEnumerator<RenderFunc<T>> GetEnumerator() => Actions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Actions).GetEnumerator();

    public Template<T> Add(RenderFunc<T> render) => this.Also(x => x.Actions.Add(render));

    public void Render(T state, StyleCollection? styles = default)
    {
        var context = new TemplateContext<T>(state, styles ?? StyleCollection.Empty());
        foreach (var render in Actions)
        {
            render(context);
        }
    }
}

public static class Template
{
    public static Template<T> For<T>() => new();
    
    public static Template<T> LineBreak<T>(this Template<T> template)
    {
        return template.Add(_ => Console.WriteLine());
    }

    public static Template<T> Whitespace<T>(this Template<T> template, int length, Func<T, string?>? style = default)
    {
        return template.Text(_ => new string(' ', length), style);
    }

    public static Template<T> Repeat<T>(this Template<T> template, Func<T, char> text, int times, Func<T, string?>? style = default)
    {
        return template.Text(x => new string(text(x), times), style);
    }

    public static Template<T> Text<T>(this Template<T> template, Func<T, string> text, Func<T, string?>? style = default)
    {
        return template.Add(ctx =>
        {
            var _style = style?.Invoke(ctx.State) is { } name ? ctx.Styles[name] : default;

            Console.Write(new string(' ', _style?.Margin?.Left ?? 0));

            using (_style?.Color?.Apply() ?? Disposable.Empty)
            {
                Console.Write(new string(' ', _style?.Padding?.Left ?? 0));
                Console.Write(_style is { Width: { } } ? text(ctx.State)[..(_style.Width.Value - 1)] + '…' : text(ctx.State));
                Console.Write(new string(' ', _style?.Padding?.Right ?? 0));
            }

            Console.Write(new string(' ', _style?.Margin?.Right ?? 0));
        });
    }

    public static Template<T> Quote<T>(this Template<T> template, char left, Func<T, string> text, char right, Func<T, string?>? textStyle = default, Func<T, string?>? quoteStyle = default)
    {
        return
            template
                .Text(_ => left.ToString(), quoteStyle)
                .Text(text, textStyle)
                .Text(_ => right.ToString(), quoteStyle);
    }
}