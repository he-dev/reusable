using System;
using System.ComponentModel;

namespace Reusable.Flexo
{
//    [Obsolete("Use Equal and other derived types.")]
//    public class Equals : PredicateExpression
//    {
//        public Equals() : base(nameof(Equals)) { }
//
//        [DefaultValue(true)]
//        public bool IgnoreCase { get; set; } = true;
//
//        public IExpression Left { get; set; }
//
//        public IExpression Right { get; set; }
//
//        protected override InvokeResult<bool> Calculate(IExpressionContext context)
//        {
//            var x = Left.InvokeWithValidation(context).ValueOrDefault();
//            var y = Right.InvokeWithValidation(context).ValueOrDefault();
//
//            if (x is string str1 && y is string str2 && IgnoreCase)
//            {
//                return (StringComparer.OrdinalIgnoreCase.Equals(str1, str2), context);
//            }
//
//            return (x.Equals(y), context);
//        }
//    }

    public abstract class Equal<T> : PredicateExpression
    {
        protected Equal(string name) : base(name) { }

        public IExpression Left { get; set; }

        public IExpression Right { get; set; }

        protected IExpression GetInput(IExpression input, IExpressionContext context)
        {
            return (input ?? context.Get(Item.For<IEqualityContext>(), c => c.Left)).Invoke(context);
        }

        protected abstract override InvokeResult<bool> Calculate(IExpressionContext context);        
    }

    public class ObjectEqual : Equal<object>
    {
        public ObjectEqual() : base(nameof(ObjectEqual)) { }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var x = GetInput(Left, context).Invoke(context).ValueOrDefault();
            var y = GetInput(Right, context).Invoke(context).ValueOrDefault();

            return (x?.Equals(y) == true, context);
        }
    }

    public class StringEqual : Equal<string>
    {
        public StringEqual() : base(nameof(StringEqual)) { }

        public bool IgnoreCase { get; set; }

        public char Trim { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var x = GetInput(Left, context).Invoke(context).ValueOrDefault<string>();
            var y = GetInput(Right, context).Invoke(context).ValueOrDefault<string>();

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
}