using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    using static ValidationResult;
    
    // ReSharper disable once UnusedTypeParameter - T is required for chaining extensions.
    public interface IValidationResult
    {
        string Expression { get; }
        
        //bool Success { get; }
        
        string Message { get; }
    }

//    internal static class ValidationResult
//    {
//        public static readonly IDictionary<bool, string> Strings = new Dictionary<bool, string>
//        {
//            [true] = "Success",
//            [false] = "Failed"
//        };
//    }

    public abstract class ValidationResult : IValidationResult
    {
//        protected ValidationResult([NotNull] string expression, bool success, [NotNull] string message)
//        {
//            Expression = expression;
//            Success = success;
//            Message = message;
//        }

        protected ValidationResult([NotNull] string expression, [NotNull] string message)
        {
            Expression = expression;
            Message = message;
        }

        public string Expression { get; }

        //public bool Success { get; }

        public string Message { get; }        

        public override string ToString() => $"{GetType().Name} | {Message} | {Expression}";

        //public static implicit operator bool(ValidationResult<T> result) => result.Success;
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
    
    public class ValidationResultCollection<T> : IEnumerable<IValidationResult>
    {
        private readonly IImmutableList<IValidationResult> _results;

        public ValidationResultCollection(T value, IImmutableList<IValidationResult> results)
        {
            Value = value;
            _results = results;
        }
        
        public T Value { get; }

        public IEnumerator<IValidationResult> GetEnumerator() => _results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator T(ValidationResultCollection<T> results) => results.Value;
    }
}