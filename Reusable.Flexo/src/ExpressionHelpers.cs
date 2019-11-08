using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    // There is already an ExpressionExtension so you use Helpers to easier tell them apart. 
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Gets only enabled expressions.
        /// </summary>
        public static IEnumerable<T> Enabled<T>(this IEnumerable<T> expressions) where T : ISwitchable
        {
            return
                from expression in expressions
                where expression.Enabled
                select expression;
        }

        /// <summary>
        /// Invokes enabled expressions.
        /// </summary>
        public static IEnumerable<IConstant> Invoke(this IEnumerable<IExpression> expressions, IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return
                from expression in expressions.Enabled()
                select expression.Invoke(context, scope);
        }

        public static IEnumerable<T> Values<T>(this IEnumerable<IConstant> constants)
        {
            return
                from constant in constants
                select constant.Value<T>();
        }

        /// <summary>
        /// Gets the value of a Constant expression if it's of the specified type T or throws an exception.
        /// </summary>
        public static T Value<T>(this IConstant constant)
        {
            if (typeof(T) == typeof(object))
            {
                return (T)constant.Value;
            }
            else
            {
                return
                    constant.Value is T value
                        ? value
                        : throw DynamicException.Create
                        (
                            "ValueType",
                            $"Constant '{constant.Id.ToString()}' should be of type '{typeof(T).ToPrettyString()}' but is '{constant.Value?.GetType().ToPrettyString()}'."
                        );
            }
        }

        public static T ValueOrDefault<T>(this IConstant constant, T defaultValue = default)
        {
            return
                constant.Value is T value
                    ? value
                    : defaultValue;
        }

        public static object ValueOrDefault(this IConstant constant, object defaultValue = default)
        {
            return constant.Value ?? defaultValue;
        }

        internal static Node<ExpressionDebugView> CreateDebugView(this IExpression expression)
        {
            return Node.Create(new ExpressionDebugView
            {
                ExpressionType = expression.GetType().ToPrettyString(),
                Name = expression.Id.ToString(),
                Description = expression.Description ?? new ExpressionDebugView().Description,
            });
        }
    }


    public static class ExpressionInvokeResultExtensions
    {
        public static string DebugViewToString(this IConstant constant, RenderTreeNodeValueCallback<ExpressionDebugView, NodePlainView> template)
        {
            return
                constant.Context.TryFindItem<Node<ExpressionDebugView>>(ExpressionContext.DebugView.ToString(), out var debugView)
                    ? debugView.Views(template).Render()
                    : throw DynamicException.Create("DebugViewNotFound", $"Could not find DebugView for '{constant}'.");
        }
    }
}