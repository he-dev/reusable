using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    // ReSharper disable once UnusedTypeParameter - We need the T to be able to chain extensions and pass the T to them.
//    public interface IWeelidationRuleResult<T>
//    {
//        [NotNull]
//        string Expression { get; }
//
//        bool Success { get; }
//
//        [CanBeNull]
//        string Message { get; }
//    }

    public interface IValidationResult<T>
    {
        string Expression { get; }
        
        bool Success { get; }
        
        string Message { get; }
    }
    
    

    internal class ValidationResult<T> : IValidationResult<T>
    {
        // ReSharper disable once StaticMemberInGenericType - this is ok because it's common to all instances.
        private static readonly IDictionary<bool, string> ResultStrings = new Dictionary<bool, string>
        {
            [true] = "Passed",
            [false] = "Failed"
        };

        public ValidationResult([NotNull] string expression, bool success, [NotNull] string message)
        {
            Expression = expression;
            Success = success;
            Message = message;
        }

        public string Expression { get; }

        public bool Success { get; }

        public string Message { get; }        

        public override string ToString() => $"{Expression} | {ResultStrings[Success]} ({Message ?? "N/A"})";

        public static implicit operator bool(ValidationResult<T> result) => result.Success;
    }
}