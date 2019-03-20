using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    public interface IExpressionEqualityComparer
    {
        IExpression Left { get; set; }

        IExpression Right { get; set; }
    }

    public abstract class Equal<T> : PredicateExpression, IExpressionEqualityComparer
    {
        protected Equal(string name) : base(name, ExpressionContext.Empty) { }

        public IExpression Left { get; set; }

        public IExpression Right { get; set; }

        protected abstract override CalculateResult<bool> Calculate(IExpressionContext context);
    }

    [Alias("==")]
    public class ObjectEqual : Equal<object>
    {
        public ObjectEqual() : base(nameof(ObjectEqual)) { }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var x = Left.Invoke(context).ValueOrDefault();
            var y = Right.Invoke(context).ValueOrDefault();

            return (x?.Equals(y) == true, context);
        }
    }

    public class StringEqual : Equal<string>
    {
        public StringEqual() : base(nameof(StringEqual)) { }

        public bool IgnoreCase { get; set; }

        public char Trim { get; set; }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var x = (string)Left.Invoke(context).ValueOrDefault();
            var y = (string)Right.Invoke(context).ValueOrDefault();

            var comparer =
                IgnoreCase
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;

            if (Trim > char.MinValue)
            {
                x = x?.Trim(Trim);
                y = y?.Trim(Trim);
            }

            return (comparer.Equals(x, y), context);
        }
    }

    public class IsEqual : PredicateExpression
    {
        public IsEqual(string name) : base(name ?? nameof(IsEqual), ExpressionContext.Empty) { }

        public object Value { get; set; }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var other = ExtensionInputOrDefault(ref context, Constant.Null).Value<object>();
            return (Constant.FromValueOrDefault(nameof(Value), Value).Invoke(context).Value<object>().Equals(other), context);
        }
    }
}