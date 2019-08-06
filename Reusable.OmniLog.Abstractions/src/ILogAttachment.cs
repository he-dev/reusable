using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogAttachment : IEquatable<ILogAttachment>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] LogEntry logEntry);
    }
    
    public abstract class LogAttachment : ILogAttachment
    {
        protected LogAttachment([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected LogAttachment()
        {
            Name = GetType().Name;
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder => { builder.DisplayValue(x => x.Name); });

        public SoftString Name { get; }

        public abstract object Compute(LogEntry logEntry);

        #region IEquatable

        public bool Equals(ILogAttachment other) => AutoEquality<ILogAttachment>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ILogAttachment);

        public override int GetHashCode() => AutoEquality<ILogAttachment>.Comparer.GetHashCode(this);

        #endregion
    }
}