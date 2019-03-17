using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.MarkupBuilder
{
    public static class MarkupElementExtensions
    {
        private static readonly ConcurrentDictionary<Type, object> MarkupElementFactoryCache = new ConcurrentDictionary<Type, object>();

        private static T CreateElement<T>(string name) where T : class, IMarkupElement
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

        [NotNull]
        public static T Element<T>([CanBeNull] this T @this, string name, Action<T> elementAction) where T : class, IMarkupElement
        {
            return @this.Element<T, object>(name, null, (element, local) => elementAction(element));
        }

        // Create a new element allows to perfom action on it with the specified local data. Useful for adding nested elements like in the console-template.
        [NotNull]
        public static T Element<T, TLocal>([CanBeNull] this T @this, string name, TLocal local, Action<T, TLocal> body) where T : class, IMarkupElement
        {
            var element = CreateElement<T>(name);

            body.Invoke(element, local);

            // @this can be null if it's a builder so return the new element instead.
            if (@this is null)
            {
                return element;
            }
            else
            {
                @this.Add(element);
                return @this;
            }
        }

        public static T Element<T>(this T @this, string name) where T : class, IMarkupElement
        {
            return @this.Element(name, e => { });
        }

        public static T Element<T>(this T @this, string name, IEnumerable<object> content) where T : class, IMarkupElement
        {
            return @this.Element(name, e => e.Append(content));
        }

        public static T Element<T>(this T @this, string name, params object[] content) where T : class, IMarkupElement
        {
            return @this.Element(name, (IEnumerable<object>)content);
        }

        /// <summary>
        /// This extension creates multiple elements with the same name for each item in the content collection. Call the .Add method to add an item to the newly created element.
        /// </summary>
        public static T Elements<T, TContent>(this T @this, string name, IEnumerable<TContent> content, Action<T, TContent> contentAction) where T : class, IMarkupElement
        {
            foreach (var item in content)
            {
                var current = CreateElement<T>(name);
                contentAction(current, item);
                @this.Add(current);
            }
            return @this;
        }

        public static T Append<T>(this T @this, IEnumerable<object> content) where T : class, IMarkupElement
        {
            foreach (var item in content)
            {
                @this.Add(item);
            }
            return @this;
        }

        public static T Append<T>(this T @this, params object[] content) where T : class, IMarkupElement
        {
            return @this.Append((IEnumerable<object>)content);
        }

        public static T Attribute<T>(this T @this, string name, string value) where T : class, IMarkupElement
        {
            @this.Attributes[name] = value;
            return @this;
        }
    }
}