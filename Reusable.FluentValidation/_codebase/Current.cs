using System;
using System.Diagnostics;

namespace Reusable.Fuse
{
    public interface ICurrent<out T>
    {
        T Value { get; }

        string MemberName { get; }

        Type ExceptionType { get; }
    }

    public class Current<T> : ICurrent<T>
    {
        [DebuggerStepThrough]
        public Current(T value, string memberName, Type exceptionType)
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