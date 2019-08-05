using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogAttachment : IEquatable<ILogAttachment>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] Log log);
    }
}