using System;
using Reusable.Extensions;

namespace Reusable.FluentValidation.Testing
{
    public class VerificationException : Exception
    {
        public VerificationException(string message, Exception innerException)
            : base(message, innerException) { }

        public VerificationException(string message)
            : base(message) { }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}