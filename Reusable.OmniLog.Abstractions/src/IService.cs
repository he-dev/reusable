using System;
using Reusable.Collections;
using Reusable.Diagnostics;

namespace Reusable.OmniLog.Abstractions
{
    public interface IService : IEquatable<IService>
    {
        bool Enabled { get; }

        [AutoEqualityProperty(StringComparison.OrdinalIgnoreCase)]
        string Name { get; }

        object? GetValue(ILogEntry logEntry);
    }

    public abstract class Service : IService
    {
        protected Service(string name) => Name = name;

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(b => { b.DisplaySingle(x => x.Name).DisplaySingle(x => x.Enabled); });

        public bool Enabled { get; set; } = true;

        public string Name { get; }

        public abstract object? GetValue(ILogEntry logEntry);

        #region IEquatable

        public bool Equals(IService? other) => AutoEquality<IService?>.Comparer.Equals(this, other);

        public override bool Equals(object? obj) => Equals(obj as IService);

        public override int GetHashCode() => AutoEquality<IService?>.Comparer.GetHashCode(this);

        #endregion
    }
}