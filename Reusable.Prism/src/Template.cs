using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Marbles;
using Reusable.Prism.Elements;

namespace Reusable.Prism;

public interface ITemplateContext<out T>
{
    ConsoleStyleSheet Css { get; }

    T State { get; }
}

public record TemplateContext<T>(T State, ConsoleStyleSheet Css) : ITemplateContext<T>;

public delegate IElement ElementFactory<in T>(ITemplateContext<T> context);

public class Template<T> : IEnumerable<ElementFactory<T>>
{
    private List<ElementFactory<T>> Factories { get; } = new();

    public IEnumerator<ElementFactory<T>> GetEnumerator() => Factories.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Factories).GetEnumerator();

    public void Add(ElementFactory<T> factory) => this.Also(x => x.Factories.Add(factory));

    public void Render(ITemplateContext<T> templateContext)
    {
        foreach (var factory in Factories)
        {
            factory(templateContext).Render(templateContext.Css);
        }
    }

    public void Render(T state, ConsoleStyleSheet? css = default) => Render(new TemplateContext<T>(state, css ?? ConsoleStyleSheet.Empty()));
}