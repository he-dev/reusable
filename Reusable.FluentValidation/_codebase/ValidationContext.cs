using System;
using System.Diagnostics;

namespace Reusable
{
    public interface IValidationContext<out T>
    {
        T Value { get; }

        string MemberName { get; }

        IValidationContext<T> Validate(Func<T, bool> validate, string message = null);
    }

    public class ValidationContext<T, TException> : IValidationContext<T> where TException : Exception
    {
        [DebuggerStepThrough]
        public ValidationContext(T value, string memberName)
        {
            Value = value;
            MemberName = memberName;
        }

        [DebuggerNonUserCode]
        public T Value { get; }

        [DebuggerNonUserCode]
        public string MemberName { get; }

        public IValidationContext<T> Validate(Func<T, bool> validate, string message)
        {
            if (validate == null) { throw new ArgumentNullException(nameof(validate)); }
            if (string.IsNullOrEmpty(message)) { throw new ArgumentNullException(nameof(message)); }

            if (validate(Value))
            {
                return this;
            }

            throw (TException)Activator.CreateInstance(typeof(TException), message);
        }
    }
}