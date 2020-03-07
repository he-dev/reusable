using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gems.Easyflow
{
    public interface IThenBuilder<T>
    {
        IElseBuilder<T> Then(Func<IEnumerable<T>, IEnumerable<T>> expression);
    }

    public interface IElseBuilder<T>
    {
        IEnumerable<T> Else(Func<IEnumerable<T>, IEnumerable<T>> expression);
    }

    [DebuggerStepThrough]
    // ReSharper disable once InconsistentNaming - this is a well known name
    internal class IIfBuilder<T> : SwitchBuilder<T>, IThenBuilder<T>, IElseBuilder<T>
    {
        private readonly Condition<T> _predicate;

        public IIfBuilder(IEnumerable<T> source, Condition<T> predicate, string? name = default) : base(source, name ?? "IIf")
        {
            _predicate = predicate;
        }

        public IElseBuilder<T> Then(Func<IEnumerable<T>, IEnumerable<T>> expression)
        {
            Case(_predicate, expression, nameof(Then));
            return this;
        }

        public IEnumerable<T> Else(Func<IEnumerable<T>, IEnumerable<T>> expression)
        {
            return Default(expression, nameof(Else));
        }
    }
}