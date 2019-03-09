using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable
{
    [PublicAPI]
    [CannotApplyEqualityOperator]
    public abstract class Primitive<T> : IEquatable<Primitive<T>>, IComparable<Primitive<T>>
    {
        private static readonly IComparer<Primitive<T>> Comparable = ComparerFactory<Primitive<T>>.Create(p => p.Value);
        
        protected Primitive(T value)
        {
            // ReSharper disable once VirtualMemberCallInConstructor - it's ok to do this here because Validate is stateless.
            Validate(Value = value);
        }

        protected abstract void Validate(T value);

        [AutoEqualityProperty]
        public T Value { get; }

        #region IEquatable

        public bool Equals(Primitive<T> other) => AutoEquality<Primitive<T>>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is T other && Equals(other);

        public override int GetHashCode() => AutoEquality<Primitive<T>>.Comparer.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(Primitive<T> other) => Comparable.Compare(this, other);

        #endregion

        public static implicit operator T(Primitive<T> primitive) => primitive.Value;
    }
}