using System;
using System.Collections.Generic;

namespace Reusable.Extensions
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> InnerExceptions(this Exception exception, bool includeCurrent = true)
        {
            if (exception == null) { throw new ArgumentNullException(nameof(exception)); }

            if (includeCurrent)
            {
                yield return exception;
            }

            var innerException = exception.InnerException;
            while (innerException != null)
            {
                yield return innerException;
                innerException = innerException.InnerException;
            }
        }
    }
}
