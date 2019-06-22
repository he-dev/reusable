using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    using static ValidationResult;
    
    // ReSharper disable once UnusedTypeParameter - T is required for chaining extensions.
    public interface IValidationResult<T>
    {
        string Expression { get; }
        
        bool Success { get; }
        
        string Message { get; }
    }

    internal static class ValidationResult
    {
        public static readonly IDictionary<bool, string> Strings = new Dictionary<bool, string>
        {
            [true] = "Success",
            [false] = "Failed"
        };
    }

    internal class ValidationResult<T> : IValidationResult<T>
    {
        public ValidationResult([NotNull] string expression, bool success, [NotNull] string message)
        {
            Expression = expression;
            Success = success;
            Message = message;
        }

        public string Expression { get; }

        public bool Success { get; }

        public string Message { get; }        

        public override string ToString() => $"{Strings[Success]} | {Message} | {Expression}";

        public static implicit operator bool(ValidationResult<T> result) => result.Success;
    }
}