using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IDataFuse<T> : IEnumerable<IDataFuseRule<T>>
    {
        [NotNull]
        IEnumerable<IDataFuseResult<T>> Validate([CanBeNull] T obj);
    }

    [PublicAPI]
    public class DataFuse<T> : IDataFuse<T>
    {
        private readonly List<IDataFuseRule<T>> _rules;

        internal DataFuse([NotNull] IEnumerable<IDataFuseRule<T>> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            _rules = rules.ToList();
        }

        public static DataFuse<T> Empty => new DataFuse<T>(Enumerable.Empty<IDataFuseRule<T>>());

        public DataFuse<T> Add([NotNull] IDataFuseRule<T> rule)
        {
            return new DataFuse<T>(_rules.Append(rule ?? throw new ArgumentNullException(nameof(rule))));
        }

        public IEnumerable<IDataFuseResult<T>> Validate(T obj)
        {
            foreach (var rule in _rules)
            {
                var result = rule.Evaluate(obj);
                yield return result;
                if (!result.Success && rule.Options.HasFlag(DataFuseOptions.StopOnFailure))
                {
                    yield break;
                }
            }
        }

        public IEnumerator<IDataFuseRule<T>> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static DataFuse<T> operator +(DataFuse<T> dataFuse, IDataFuseRule<T> rule)
        {
            return dataFuse.Add(rule);
        }
    }

    public static class DataFuse
    {
        public static IDataFuse<T> For<T>() => DataFuse<T>.Empty;
    }
}
