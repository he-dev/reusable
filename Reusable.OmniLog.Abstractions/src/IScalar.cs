using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface IScalar : IEquatable<IScalar>
    {
        bool Enabled { get; }
        
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] LogEntry logEntry);
    }
    
    public abstract class Scalar : IScalar
    {
        protected Scalar([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected Scalar()
        {
            Name = GetType().Name;
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(b => { b.DisplayScalar(x => x.Name); });

        public bool Enabled { get; set; } = true;
        
        public SoftString Name { get; }

        public abstract object Compute(LogEntry logEntry);

        #region IEquatable

        public bool Equals(IScalar other) => AutoEquality<IScalar>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as IScalar);

        public override int GetHashCode() => AutoEquality<IScalar>.Comparer.GetHashCode(this);

        #endregion
    }
}