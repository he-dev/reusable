using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class ComparerExpression : PredicateExpression, IExtension<object>
    {
        private readonly Func<int, bool> _predicate;

        protected ComparerExpression(string name, [NotNull] Func<int, bool> predicate)
            : base(name) => _predicate = predicate;

        [JsonRequired]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            if (context.TryPopExtensionInput(out object input))
            {
                var result = Comparer<object>.Default.Compare(input, Value.Invoke(context).Value);
                return (Name, _predicate(result), context);
            }
            else
            {
                throw new InvalidOperationException($"{Name.ToString()} can be used only as an extension.");
            }
        }

//        private static bool TryCompare<T>(IExpression x, IExpression y, out int result)
//        {
//            if (x is Constant<T> && y is Constant<T>)
//            {
//                result = ComparerFactory<IExpression>.Create(c => c.ValueOrDefault<T>()).Compare(x, y);
//                return true;
//            }
//            else
//            {
//                result = default;
//                return false;
//            }
//        }
    }
}