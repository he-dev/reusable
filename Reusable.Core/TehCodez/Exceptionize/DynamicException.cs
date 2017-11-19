using System;
using JetBrains.Annotations;

namespace Reusable.Exceptionize
{
    public abstract class DynamicException : Exception
    {
        protected DynamicException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Gets the default dynamic exception factory. It uses an internal cache for each exception type.
        /// </summary>
        [NotNull]
        public static IDynamicExceptionFactory Factory => DynamicExceptionFactory.Default;
    }   
}