using System;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogAttachment : IEquatable<ILogAttachment>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] ILog log);
    }
}