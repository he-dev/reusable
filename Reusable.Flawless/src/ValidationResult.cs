using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public interface IValidationResult
    {
        string Expression { get; }

        string Message { get; }
    }
    
    public interface IValidationFailure { }

    public abstract class ValidationResult : IValidationResult
    {
        protected ValidationResult([NotNull] string expression, [NotNull] string message)
        {
            Expression = expression;
            Message = message;
        }

        public string Expression { get; }

        public string Message { get; }

        public override string ToString() => $"{GetType().Name} | {Message} | {Expression}";

        public static implicit operator bool(ValidationResult result) => result is ValidationSuccess;
    }

    public class ValidationSuccess : ValidationResult
    {
        public ValidationSuccess([NotNull] string expression, [NotNull] string message)
            : base(expression, message) { }
    }

    public class ValidationWarning : ValidationResult, IValidationFailure
    {
        public ValidationWarning([NotNull] string expression, [NotNull] string message)
            : base(expression, message) { }
    }

    public class ValidationError : ValidationResult, IValidationFailure
    {
        public ValidationError([NotNull] string expression, [NotNull] string message)
            : base(expression, message) { }
    }
}