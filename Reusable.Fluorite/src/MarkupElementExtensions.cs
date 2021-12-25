using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Fluorite.Formatters;
using Reusable.Fluorite.Html;

namespace Reusable.Fluorite;

public static class HtmlElementFactory
{
    private static readonly ConcurrentDictionary<Type, object> MarkupElementFactoryCache = new();

    public static T CreateElement<T>(string name) where T : class, IHtmlElement
    {
        var create = (Func<string, T>)MarkupElementFactoryCache.GetOrAdd(typeof(T), _ =>
        {
            var parameterType = typeof(string);

            var constructor = typeof(T).GetConstructor(new[] { parameterType });

            if (constructor is null)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"ConstructorNotFound{nameof(Exception)}",
                    $"Markup elements must have a constructor that takes one parameter 'string: name'. Affected type: {typeof(T).ToPrettyString().QuoteWith("'")}",
                    null
                );
            }

            var nameParameter = Expression.Parameter(parameterType, nameof(name));

            return Expression.Lambda<Func<string, T>>(
                Expression.New(constructor, nameParameter),
                nameParameter
            ).Compile();
        });

        return create(name);
    }
}

public static class MarkupElementExtensions
{
    /// <summary>
    /// This extension creates multiple elements with the same name for each item in the content collection. Call the .Add method to add an item to the newly created element.
    /// </summary>
    public static IHtmlElement Elements<T>(this IHtmlElement parent, string name, IEnumerable<T> content, Action<IHtmlElement, T>? onEach = default)
    {
        onEach ??= (e, c) => e.Add(c!);
        
        foreach (var item in content)
        {
            // ReSharper disable once MustUseReturnValue
            HtmlElement
                .Create(name)
                .Also(e => onEach(e, item))
                .Also(parent.Add);
        }

        return parent;
    }

    // Creates a new element and allows to perform an action on it with the specified local data. Useful for adding nested elements.
    public static IHtmlElement Element<T>(this IHtmlElement parent, string name, [DisallowNull] T content, Action<IHtmlElement, T>? onEach = default)
    {
        onEach ??= (e, c) => e.Add(c!);

        return
            HtmlElement
                .Create(name)
                .Also(e => onEach(e, content))
                .Also(parent.Add);
    }

    public static IHtmlElement Element(this IHtmlElement parent, string name, Action<IHtmlElement> action)
    {
        return HtmlElement.Create(name).Also(action).Also(parent.Add);
    }

    public static IHtmlElement Element(this IHtmlElement parent, string name)
    {
        return HtmlElement.Create(name).Also(parent.Add);
    }

    public static IHtmlElement Element(this IHtmlElement parent, string name, IEnumerable<object> content)
    {
        return parent.Element(name, e =>
        {
            foreach (var item in content)
            {
                e.Add(item);
            }
        });
    }

    public static IHtmlElement Element(this IHtmlElement parent, string name, params object[] content)
    {
        return parent.Element(name, content.AsEnumerable());
    }

    public static IHtmlElement Text(this IHtmlElement parent, params object[] content)
    {
        return parent.Also(e =>
        {
            foreach (var item in content)
            {
                e.Add(item);
            }
        });
    }


    public static T Attribute<T>(this T @this, string name, string value) where T : class, IHtmlElement
    {
        @this.Attributes[name] = value;
        return @this;
    }

    public static string ToHtml<T>(this T element, HtmlFormatting formatting) where T : IHtmlElement
    {
        return element.ToString(default, new HtmlFormatProvider(new HtmlRenderer(formatting)));
    }

    public static string ToHtml<T>(this T element) where T : IHtmlElement
    {
        return element.ToString(default, new HtmlFormatProvider(new HtmlRenderer(HtmlFormatting.Empty)));
    }
}