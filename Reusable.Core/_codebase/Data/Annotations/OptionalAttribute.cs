using System;

namespace Reusable.Data.Annotations
{
    /// <summary>
    /// Indicates that a property is optional. You should provide a default value in this case.
    /// </summary>
    [Obsolete("Use the RequiredAttribute when possible.")]
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalAttribute : Attribute { }
}
