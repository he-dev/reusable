using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.Exceptionize
{
    public static class DynamicExceptionFactoryExtensions
    {
        public static Exception CreateDynamicException(this IDynamicExceptionFactory factory, string name, string message)
        {
            return factory.CreateDynamicException(name, message, null);
        }

        /// <summary>
        /// Creates a DynamicException from the specified template.
        /// </summary>
        [NotNull, ContractAnnotation("factory: null => halt; template: null => halt")]
        public static Exception CreateDynamicException([NotNull] this IDynamicExceptionFactory factory, [NotNull] IDynamicExceptionTemplate template)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (template == null) throw new ArgumentNullException(nameof(template));

            return factory.CreateDynamicException(template.Name(), template.Message, template.InnerException);
        }

        /// <summary>
        /// Creates a DynamicException with the name of the calling method, and with the specified message and optionally an inner exception.
        /// </summary>
        /// <returns></returns>
        [NotNull, ContractAnnotation("factory: null => halt")]
        public static Exception CreateDynamicException([NotNull] this IDynamicExceptionFactory factory, string message, Exception? innerException = null, [CallerMemberName] string? memberName = null)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            return factory.CreateDynamicException($"{memberName}{nameof(Exception)}", message, innerException);
        }       

        public static Exception ToDynamicException(this (string Name, string Message) template)
        {
            return DynamicException.Create(template.Name, template.Message);
        }

        public static Exception ToDynamicException(this (string Name, string? Message, Exception? InnerException) template)
        {
            return DynamicException.Create(template.Name, template.Message, template.InnerException);
        }
    }
}