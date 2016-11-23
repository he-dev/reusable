using System;

namespace Reusable.Fuse.Testing
{
    public class VerificationException : Exception
    {
        public VerificationException(string message, Exception innerException)
            : base(message, innerException) { }

        public VerificationException(string message)
            : base(message) { }        
    }
}