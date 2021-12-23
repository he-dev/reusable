using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Htmlize.Formatters;
using Reusable.Htmlize.Html;

namespace Reusable.Htmlize;

public static class MarkupElementExtensions
{
    private static readonly ConcurrentDictionary<Type, object> MarkupElementFactoryCache = new ConcurrentDictionary<Type, object>();

    private static T CreateElement<T>(string name) where T : class, IHtmlElement
    {
        var create = (Func<string, T>)MarkupElementFactoryCache.GetOrAdd(typeof(T), markupElementType =>
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

    public static T Element<T>(this T? parent, string name, Action<T> elementAction) where T : class, IHtmlElement
    {
        return parent.Element<T, object>(name, null, (element, local) => elementAction(element));
    }

    // Create a new element allows to perform action on it with the specified local data. Useful for adding nested elements like in the console-template.
    public static T Element<T, TLocal>(this T? parent, string name, TLocal local, Action<T, TLocal> body) where T : class, IHtmlElement
    {
        var element = CreateElement<T>(name);

        body.Invoke(element, local);

        // parent can be null if it's a builder so return the new element instead.
        if (parent is null)
        {
            return element;
        }
        else
        {
            parent.Add(element);
            return parent;
        }
    }

    public static T Element<T>(this T @this, string name) where T : class, IHtmlElement
    {
        return @this.Element(name, e => { });
    }

    public static T Element<T>(this T @this, string name, IEnumerable<object> content) where T : class, IHtmlElement
    {
        return @this.Element(name, e => e.Append(content));
    }

    public static T Element<T>(this T @this, string name, params object[] content) where T : class, IHtmlElement
    {
        return @this.Element(name, (IEnumerable<object>)content);
    }

    /// <summary>
    /// This extension creates multiple elements with the same name for each item in the content collection. Call the .Add method to add an item to the newly created element.
    /// </summary>
    public static T Elements<T, TContent>(this T @this, string name, IEnumerable<TContent> content, Action<T, TContent> contentAction) where T : class, IHtmlElement
    {
        foreach (var item in content)
        {
            var current = CreateElement<T>(name);
            contentAction(current, item);
            @this.Add(current);
        }
        return @this;
    }

    public static T Append<T>(this T parent, IEnumerable<object> content) where T : class, IHtmlElement
    {
        foreach (var item in content)
        {
            parent.Add(item);
        }
        return parent;
    }

    public static T Append<T>(this T parent, params object[] content) where T : class, IHtmlElement
    {
        return parent.Append(content.AsEnumerable());
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