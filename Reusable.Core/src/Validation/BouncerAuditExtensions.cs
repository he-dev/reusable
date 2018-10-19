using System;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Validation
{
    public static class BouncerAuditExtensions
    {
        [CanBeNull]
        public static T ThrowIfInvalid<T>([NotNull] this BouncerAudit<T> bouncerAudit)
        {
            if (bouncerAudit == null) throw new ArgumentNullException(nameof(bouncerAudit));

            return
                bouncerAudit
                    ? bouncerAudit
                    : throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Object of type '{typeof(T).ToPrettyString()}' does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                        $"{bouncerAudit[false].Select(Func.ToString).Join(Environment.NewLine)}"
                    );
        }
    }
}