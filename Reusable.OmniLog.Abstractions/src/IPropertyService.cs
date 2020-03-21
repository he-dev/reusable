using System;
using Reusable.Collections;
using Reusable.Diagnostics;

namespace Reusable.OmniLog.Abstractions
{
    public interface IPropertyService : IEquatable<IPropertyService>
    {
        bool Enabled { get; }

        [AutoEqualityProperty(StringComparison.OrdinalIgnoreCase)]
        string Name { get; }

        object? GetValue(ILogEntry logEntry);
    }

    public abstract class PropertyService : IPropertyService
    {
        protected PropertyService(string name) => Name = name;

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(b => { b.DisplaySingle(x => x.Name).DisplaySingle(x => x.Enabled); });

        public bool Enabled { get; set; } = true;

        public string Name { get; }

        public abstract object? GetValue(ILogEntry logEntry);

        #region IEquatable

        public bool Equals(IPropertyService? other) => AutoEquality<IPropertyService?>.Comparer.Equals(this, other);

        public override bool Equals(object? obj) => Equals(obj as IPropertyService);

        public override int GetHashCode() => AutoEquality<IPropertyService?>.Comparer.GetHashCode(this);

        #endregion
    }
}