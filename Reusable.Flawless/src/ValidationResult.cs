using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public interface IValidationResult
    {
        string Expression { get; }

        string Message { get; }
    }

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

        public static implicit operator bool(ValidationResult result) => result is Information;
    }

    public class Information : ValidationResult
    {
        public Information([NotNull] string expression, [NotNull] string message)
            : base(expression, message) { }
    }

    public class Warning : ValidationResult
    {
        public Warning([NotNull] string expression, [NotNull] string message)
            : base(expression, message) { }
    }

    public class Error : ValidationResult
    {
        public Error([NotNull] string expression, [NotNull] string message)
            : base(expression, message) { }
    }
}