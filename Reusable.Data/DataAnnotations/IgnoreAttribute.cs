using System;

namespace Reusable.Data.DataAnnotations
{
    /// <summary>
    /// Informs the settings loader to ignore a class or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute { }
}
