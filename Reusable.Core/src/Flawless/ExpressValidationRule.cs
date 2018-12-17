using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public interface IExpressValidationRule<T>
    {
        ExpressValidationOptions Options { get; }
        
        ExpressValidationResult<T> Evaluate([CanBeNull] T obj);
        
        string GetMessage(T obj);
    }

    internal class ExpressValidationRule<T> : IExpressValidationRule<T>
    {
        private readonly Lazy<string> _expressionString;
        private readonly Lazy<Func<T, bool>> _predicate;
        private readonly Func<T, string> _createMessage;

        public ExpressValidationRule([NotNull] Expression<Func<T, bool>> predicate, [NotNull] Func<T, string> createMessage, ExpressValidationOptions options)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = Lazy.Create(predicate.Compile);
            _expressionString = Lazy.Create(ExpressValidationRulePrettifier.Prettify(predicate).ToString);
            _createMessage = createMessage ?? throw new ArgumentNullException(nameof(createMessage));
            Options = options;
        }

        public ExpressValidationOptions Options { get; }

        public ExpressValidationResult<T> Evaluate(T obj) => new ExpressValidationResult<T>(ToString(), _predicate.Value(obj), _createMessage(obj));
        
        public string GetMessage(T obj) => _createMessage(obj);

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(ExpressValidationRule<T> rule) => rule?.ToString();
    }
}