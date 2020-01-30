using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface IService : IEquatable<IService>
    {
        bool Enabled { get; }

        [AutoEqualityProperty]
        SoftString Name { get; }

        object? GetValue(LogEntry logEntry);
    }

    public abstract class Service : IService
    {
        protected Service(string name) => Name = name!;

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(b => { b.DisplayScalar(x => x.Name); });

        public bool Enabled { get; set; } = true;

        public SoftString Name { get; }

        public abstract object? GetValue(LogEntry logEntry);

        #region IEquatable

        public bool Equals(IService? other) => AutoEquality<IService?>.Comparer.Equals(this, other);

        public override bool Equals(object? obj) => Equals(obj as IService);

        public override int GetHashCode() => AutoEquality<IService?>.Comparer.GetHashCode(this);

        #endregion
    }
}