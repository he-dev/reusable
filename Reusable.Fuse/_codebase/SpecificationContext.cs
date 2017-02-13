using System;
using System.Diagnostics;

namespace Reusable.Fuse
{
    public interface ISpecificationContext<out T>
    {
        T Value { get; }

        string MemberName { get; }

        Type ExceptionType { get; }
    }

    public class SpecificationContext<T> : ISpecificationContext<T>
    {
        [DebuggerStepThrough]
        public SpecificationContext(T value, string memberName, Type exceptionType)
        {
            Value = value;
            MemberName = memberName;
            ExceptionType = exceptionType;
        }

        [DebuggerNonUserCode]
        public T Value { get; }

        [DebuggerNonUserCode]
        public string MemberName { get; }

        public Type ExceptionType { get; }

    }
}