using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.Flawless
{
    public interface IValidationRule<T, in TContext>
    {
        ValidationRuleOption Option { get; }
        
        

        IValidationResult<T> Evaluate([CanBeNull] T obj, TContext context);
    }

    public class ValidationRuleOption : Option<ValidationRuleOption>
    {
        public ValidationRuleOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly ValidationRuleOption Ensure = CreateWithCallerName();

        public static readonly ValidationRuleOption Require = CreateWithCallerName();
    }

    internal class ValidationRule<T, TContext> : IValidationRule<T, TContext>
    {
        private readonly Lazy<string> _expressionString;
        private readonly Lazy<Func<T, bool>> _predicate;
        private readonly Func<T, string> _createMessage;

        public ValidationRule([NotNull] Expression<Func<T, bool>> predicate, [NotNull] Func<T, string> message, ValidationRuleOption option)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = Lazy.Create(predicate.Compile);
            _expressionString = Lazy.Create(ValidationParameterPrettifier.Prettify(predicate).ToString);
            _createMessage = _ => "Blub!"; // message ?? throw new ArgumentNullException(nameof(message));
            Option = option;
        }

        public ValidationRuleOption Option { get; }

        public IValidationResult<T> Evaluate(T obj, TContext context)
        {
            return new ValidationResult<T>(ToString(), _predicate.Value(obj), _createMessage(obj));
        }

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(ValidationRule<T, TContext> rule) => rule?.ToString();
    }

    public static class ValidationRule
    {
        public static ValidationRuleBuilder<T> Ensure<T>(T obj) => new ValidationRuleBuilder<T>(ValidationRuleOption.Ensure);
        
        public static ValidationRuleBuilder<T> Require<T>(T obj) => new ValidationRuleBuilder<T>(ValidationRuleOption.Require);
    }

    public interface IValidationRuleBuilder<T>
    {
        IValidationRule<T, TContext> Build<TContext>();
    }

    public class ValidationRuleBuilder<T> //: IValidationRuleBuilder<T>
    {
        private readonly ValidationRuleOption _option;

        private Expression<Func<T, bool>> _predicate;

        public ValidationRuleBuilder(ValidationRuleOption option)
        {
            _option = option;
            _predicate = _ => true;
        }

        public ValidationRuleBuilder<T> Predicate(Expression<Func<T, bool>> expression)
        {
            _predicate = expression;
            return this;
        }

        public ValidationRuleBuilder<T> Message(Expression<Func<string>> message)
        {
            return this;
        }
        
        [NotNull]
        public IValidationRule<T, TContext> Build<TContext>()
        {
            return new ValidationRule<T, TContext>(_predicate, default, _option);
        }
    }
}