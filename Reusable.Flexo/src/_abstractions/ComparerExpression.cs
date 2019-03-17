using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class ComparerExpression : Expression
    {
        private readonly Func<int, bool> _predicate;

        protected ComparerExpression(string name, [NotNull] Func<int, bool> predicate) : base(name) => _predicate = predicate;

        [JsonRequired]
        public IExpression Left { get; set; }

        [JsonRequired]
        public IExpression Right { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var x = Left.Invoke(context);
            var y = Right.Invoke(context);

            var result = default(int);

            var compared =
                TryCompare<int>(x, y, out result) ||
                TryCompare<float>(x, y, out result) ||
                TryCompare<double>(x, y, out result) ||
                TryCompare<string>(x, y, out result) ||
                TryCompare<decimal>(x, y, out result) ||
                TryCompare<System.DateTime>(x, y, out result) ||
                TryCompare<System.TimeSpan>(x, y, out result) ||
                TryCompare<object>(x, y, out result);

            if (compared)
            {
                return Constant.FromValue(Name, _predicate(result));
            }

            throw new InvalidOperationException($"Expressions '{x.Name}' & '{y.Name}' are not comparable.");
        }

        private static bool TryCompare<T>(IExpression x, IExpression y, out int result)
        {
            if (x is Constant<T> && y is Constant<T>)
            {
                result = ComparerFactory<IExpression>.Create(c => c.ValueOrDefault<T>()).Compare(x, y);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}