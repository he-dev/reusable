using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

        public static IEnumerable<IConstant> Invoke(this IEnumerable<IExpression> expressions, IImmutableContainer context)
        {
            return
                from expression in expressions.Enabled()
                select expression.Invoke(context);
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
                            $"Expected {typeof(Constant<T>).ToPrettyString()} but found {constant.GetType().ToPrettyString()}."
                        );
            }
        }

        public static T ValueOrDefault<T>(this IConstant expression)
        {
            return
                expression is IConstant constant && constant.Value is T value
                    ? value
                    : default;
        }

        public static object ValueOrDefault(this IConstant expression)
        {
            return
                expression is IConstant constant
                    ? constant.Value
                    : default;
        }

        // Resolves the actual expression in case it's Ref
        [NotNull]
        public static IExpression Resolve(this IExpression expression)
        {
            while (expression is Ref @ref)
            {
                expression = @ref.Invoke().Value<IExpression>();
            }

            return expression;
        }

        internal static Node<ExpressionDebugView> CreateDebugView(this IExpression expression)
        {
            return Node.Create(new ExpressionDebugView
            {
                ExpressionType = expression.GetType().ToPrettyString(),
                Name = expression.Name.ToString(),
                Description = expression.Description ?? new ExpressionDebugView().Description,
            });
        }

        internal static object ThisOuterOrDefault(this IExpression expression)
        {
            var thisOuterValue =
                expression is IExtension extension
                    ? extension.ThisOuter
                    : default;

            switch (thisOuterValue)
            {
                case null: return default;
                case IExpression e: return e;
                case IEnumerable<IExpression> c: return c;
                default:
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(IExtension.ThisOuter),
                        message:
                        $"'This' value is of type '{thisOuterValue.GetType().ToPrettyString()}' " +
                        $"but it must be either an '{typeof(IExpression).ToPrettyString()}' " +
                        $"or an '{typeof(IEnumerable<IExpression>).ToPrettyString()}'"
                    );
            }
        }

        public static ExpressionInvokeResult Invoke(this IExpression expression, Func<IImmutableContainer, IImmutableContainer> alterContext)
        {
            using (Expression.BeginScope(alterContext ?? (_ => _)))
            {
                try
                {
                    return new ExpressionInvokeResult
                    {
                        Constant = expression.Invoke(TODO), 
                        Contexts = DumpContexts()
                    };
                }
                catch (Exception inner)
                {
                    throw new ExpressionException(inner)
                    {
                        Contexts = DumpContexts()
                    };
                }
            }
        }

        private static IList<IImmutableContainer> DumpContexts() => Expression.Scope.Enumerate().Select(scope => scope.Context).ToList();
    }

    public class ExpressionInvokeResult
    {
        public IConstant Constant { get; set; }

        public IList<IImmutableContainer> Contexts { get; set; }
    }

    public static class ExpressionInvokeResultExtensions
    {
        public static string DebugViewToString(this ExpressionInvokeResult result, RenderTreeNodeValueCallback<ExpressionDebugView, NodePlainView> template)
        {
            return
                result
                    .Contexts
                    .First()
                    .DebugView()
                    .Views(template)
                    .Render();
        }
    }
}