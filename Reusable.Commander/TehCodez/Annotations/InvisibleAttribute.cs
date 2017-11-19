using System;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Reusable.Commander
{
    /// <summary>
    /// This attribute can be set on commands that should not be found by the command-executor.
    /// </summary>
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InvisibleAttribute : Attribute
    {
        
    }
}