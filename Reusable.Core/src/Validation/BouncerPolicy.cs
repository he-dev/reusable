using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IBouncerPolicy<T>
    {
        BouncerPolicyOptions Options { get; }
        
        BouncerPolicyCheck<T> Evaluate([CanBeNull] T obj);
        
        string GetMessage(T obj);
    }

    internal class BouncerPolicy<T> : IBouncerPolicy<T>
    {
        private readonly Lazy<string> _expressionString;
        private readonly Lazy<Func<T, bool>> _policy;
        private readonly Func<T, string> _createMessage;

        public BouncerPolicy([NotNull] Expression<Func<T, bool>> policy, [NotNull] Func<T, string> createMessage, BouncerPolicyOptions options)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            _policy = Lazy.Create(policy.Compile);
            _expressionString = Lazy.Create(BouncerPolicyExpressionPrettifier.Prettify(policy).ToString);
            _createMessage = createMessage ?? throw new ArgumentNullException(nameof(createMessage));
            Options = options;
        }

        public BouncerPolicyOptions Options { get; }

        public BouncerPolicyCheck<T> Evaluate(T obj) => new BouncerPolicyCheck<T>(ToString(), _policy.Value(obj), _createMessage(obj));
        
        //public bool Evaluate(T obj) => _policy.Value(obj);

        public string GetMessage(T obj) => _createMessage(obj);

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(BouncerPolicy<T> rule) => rule?.ToString();
    }
}