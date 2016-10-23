using System;
using Reusable.Extensions;

namespace Reusable.FluentValidation
{
    public class ValidationException : Exception
    {
        public ValidationException(string message, Exception innerException)
            : base(message, innerException) { }

        public ValidationException(string message)
            : base(message) { }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}