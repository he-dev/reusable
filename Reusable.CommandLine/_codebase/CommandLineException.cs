using System;

namespace Reusable.Shelly
{
    public class CommandLineException : Exception
    {
        internal CommandLineException(int exitCode, string message, Exception innerException) : base(message, innerException)
        {
            ExitCode = exitCode;
        }

        internal CommandLineException(int exitCode, string message) : this(exitCode, message, null)
        {
        }

        public int ExitCode { get; }
    }
}
