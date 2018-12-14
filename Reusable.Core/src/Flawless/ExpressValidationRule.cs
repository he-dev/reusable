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
        private readonly Lazy<Func<T, bool>> _policy;
        private readonly Func<T, string> _createMessage;

        public ExpressValidationRule([NotNull] Expression<Func<T, bool>> policy, [NotNull] Func<T, string> createMessage, ExpressValidationOptions options)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            _policy = Lazy.Create(policy.Compile);
            _expressionString = Lazy.Create(ExpressValidationRulePrettifier.Prettify(policy).ToString);
            _createMessage = createMessage ?? throw new ArgumentNullException(nameof(createMessage));
            Options = options;
        }

        public ExpressValidationOptions Options { get; }

        public ExpressValidationResult<T> Evaluate(T obj) => new ExpressValidationResult<T>(ToString(), _policy.Value(obj), _createMessage(obj));
        
        public string GetMessage(T obj) => _createMessage(obj);

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(ExpressValidationRule<T> rule) => rule?.ToString();
    }
}