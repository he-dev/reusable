using System;
using Reusable.Essentials.Collections;

namespace Reusable.Wiretap.Abstractions;

public interface IPropertyService : IEquatable<IPropertyService>
{
    bool Enabled { get; }

    void Invoke(ILogEntry entry);
}

public abstract class PropertyService : IPropertyService
{
    //private string DebuggerDisplay() => this.ToDebuggerDisplayString(b => { b.DisplaySingle(x => x.Name).DisplaySingle(x => x.Enabled); });

    public bool Enabled { get; set; } = true;

    public abstract void Invoke(ILogEntry entry);

    #region IEquatable

    public bool Equals(IPropertyService? other) => AutoEquality<IPropertyService?>.Comparer.Equals(this, other);

    public override bool Equals(object? obj) => Equals(obj as IPropertyService);

    public override int GetHashCode() => AutoEquality<IPropertyService?>.Comparer.GetHashCode(this);

    #endregion
}