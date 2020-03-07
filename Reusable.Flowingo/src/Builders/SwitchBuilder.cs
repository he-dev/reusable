using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gems.Easyflow
{
    public interface ISwitchBuilder<T>
    {
        ISwitchBuilder<T> Case(Condition<T> predicate, Func<IEnumerable<T>, IEnumerable<T>> expression, string? name = default);

        IEnumerable<T> Default(Func<IEnumerable<T>, IEnumerable<T>> expression, string? name = default);
    }
    
    internal class SwitchBuilder<T> : ISwitchBuilder<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly string _name;
        private readonly List<(Condition<T> Predicate, Func<IEnumerable<T>, IEnumerable<T>> Body, string Name)> _cases;

        [DebuggerStepThrough]
        public SwitchBuilder(IEnumerable<T> source, string? name = default)
        {
            _source = source;
            _name = name ?? "Switch";
            _cases = new List<(Condition<T> Predicate, Func<IEnumerable<T>, IEnumerable<T>> Body, string Name)>();
        }

        public ISwitchBuilder<T> Case(Condition<T> predicate, Func<IEnumerable<T>, IEnumerable<T>> expression, string? name = default)
        {
            _cases.Add((predicate, expression, name ?? nameof(Case)));
            return this;
        }

        public IEnumerable<T> Default(Func<IEnumerable<T>, IEnumerable<T>> expression, string? name = default)
        {
            var cases =
                from x in _source
                from c in _cases
                select (x, c);

            foreach (var (x, c) in cases)
            {
                if (c.Predicate(x))
                {
                    yield return x;
                    yield break;
                }
            }

            foreach (var x in expression(_source))
            {
                yield return x;
            }
        }
    }
}