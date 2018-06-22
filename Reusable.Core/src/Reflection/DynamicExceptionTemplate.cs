using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Reflection
{
    public interface IDynamicExceptionTemplate
    {
        [CanBeNull]
        string Message { get; }

        [CanBeNull]
        Exception InnerException { get; }
    }

    public abstract class DynamicExceptionTemplate : IDynamicExceptionTemplate
    {
        public abstract string Message { get; }

        public Exception InnerException { get; set; }

        [NotNull, ContractAnnotation("template: null => halt")]
        public static implicit operator Exception([NotNull] DynamicExceptionTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            return template.ToDynamicException();
        }
    }    

    public static class DynamicExceptionTemplateExtensions
    {
        public static string Name<T>(this T template) where T : IDynamicExceptionTemplate
        {
            return Regex.Replace(typeof(T).Name, $"({nameof(Exception)})?Template$", nameof(Exception));
        }

        public static Exception ToDynamicException(this IDynamicExceptionTemplate template)
        {
            return DynamicException.Factory.CreateDynamicException(template.Name(), template.Message, template.InnerException);
        }
    }
}