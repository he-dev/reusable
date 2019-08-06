using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface IComputable : IEquatable<IComputable>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] LogEntry logEntry);
    }
    
    public abstract class Computable : IComputable
    {
        protected Computable([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected Computable()
        {
            Name = GetType().Name;
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder => { builder.DisplayValue(x => x.Name); });

        public SoftString Name { get; }

        public abstract object Compute(LogEntry logEntry);

        #region IEquatable

        public bool Equals(IComputable other) => AutoEquality<IComputable>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as IComputable);

        public override int GetHashCode() => AutoEquality<IComputable>.Comparer.GetHashCode(this);

        #endregion
    }
}