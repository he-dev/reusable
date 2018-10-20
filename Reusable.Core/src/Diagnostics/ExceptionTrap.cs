using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Diagnostics
{
    public interface IExceptionTrap
    {
        void Throw
        (
            [CanBeNull] string id = null,
            [CanBeNull] Func<bool> test = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0
        );
    }

    public class ExceptionTrap : IExceptionTrap
    {
        private readonly IEnumerable<IExceptionTrigger> _triggers;

        public ExceptionTrap(IEnumerable<IExceptionTrigger> triggers)
        {
            _triggers = triggers;
        }

        // Caches trap keys because creating them requires reflection via StackTrace and we don't want to create a bottleneck.
        private readonly ConcurrentDictionary<string, (string Namespace, string Type, string Member, string Id)> _traps = new ConcurrentDictionary<string, (string, string, string, string)>();

        // Prevent inlining just in case so that the stack-trace does not change.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Throw
        (
            string id = null,
            Func<bool> test = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0
        )
        {
            var key = $"{callerMemberName} in {callerFilePath} at {callerLineNumber}";
            var trap = _traps.GetOrAdd(key, _ =>
            {
                // 1 - Throw
                // 2 - GetOrAdd
                var method = new StackTrace().GetFrame(2).GetMethod();

                // ReSharper disable once PossibleNullReferenceException - I doubt 'DeclaringType' is ever 'null'.
                return
                (
                    method.DeclaringType.Namespace,
                    method.DeclaringType.Name,
                    method.Name,
                    id
                );
            });

            var trigger = test is null || test() ? _triggers.FirstOrDefault(x => x.CanThrow(trap)) : default;
            if (!(trigger is null))
            {
                throw DynamicException.Create(trigger.Exception ?? "Diagnostic", $"This is a diagnostic exception for {trigger}. {trigger.Message}");
            }
        }
    }

    public interface IExceptionTrigger
    {
        [CanBeNull]
        string Exception { get; }

        [CanBeNull]
        string Message { get; }

        bool CanThrow((string Namespace, string Type, string Member, string Id) trap);
    }

    [PublicAPI]
    [UsedImplicitly]
    public abstract class ExceptionTrigger : IExceptionTrigger, IDisposable
    {
        private readonly IEnumerator<int> _sequence;

        private bool _hasElements;

        protected ExceptionTrigger(IEnumerable<int> sequence, int count = 0)
        {
            _sequence = (count > 0 ? sequence.Take(count) : sequence).GetEnumerator();
            _hasElements = _sequence.MoveNext();
        }

        protected int Current => _sequence.Current;

        public bool Enabled { get; set; } = true;

        #region IExceptionTrigger

        public string Exception { get; set; }

        public string Message { get; set; }

        #endregion

        public string Namespace { get; set; }

        public string Type { get; set; }

        public string Member { get; set; }

        public string Id { get; set; }

        public bool CanThrow((string Namespace, string Type, string Member, string Id) trap)
        {
            if (Enabled && TrapMatches(trap) && _hasElements && CanThrow())
            {
                _hasElements = _sequence.MoveNext();
                return true;
            }

            return false;
        }

        protected abstract bool CanThrow();

        public abstract override string ToString();

        private bool TrapMatches((string Namespace, string Type, string Member, string Id) trap)
        {
            return
                NameMatches(Namespace, trap.Namespace) &&
                NameMatches(Type, trap.Type) &&
                NameMatches(Member, trap.Member) &&
                NameMatches(Id, trap.Id);
        }

        private static bool NameMatches(string name, string trap) => name is null || SoftString.Comparer.Equals(name, trap);

        public void Dispose() => _sequence.Dispose();
    }


    [UsedImplicitly]
    public class CountedTrigger : ExceptionTrigger
    {
        private int _counter;

        public CountedTrigger(IEnumerable<int> sequence, int count = default) : base(sequence, count)
        {
        }

        protected override bool CanThrow()
        {
            if (++_counter == Current)
            {
                _counter = 0;
                return true;
            }

            return false;
        }

        public override string ToString() => $"{nameof(CountedTrigger)}: {_counter}";
    }

    [UsedImplicitly]
    public class DelayedTrigger : ExceptionTrigger
    {
        private Stopwatch _stopwatch;

        public DelayedTrigger(IEnumerable<int> sequence, int count = default) : base(sequence, count)
        {
        }

        private TimeSpan Delay => TimeSpan.FromSeconds(Current);

        protected override bool CanThrow()
        {
            _stopwatch = _stopwatch ?? Stopwatch.StartNew();
            if (_stopwatch.Elapsed >= Delay)
            {
                _stopwatch.Restart();
                return true;
            }

            return false;
        }

        public override string ToString() => $"{nameof(DelayedTrigger)}: {Delay} ({_stopwatch.Elapsed})";
    }

    public interface ISequence<out T> : IEnumerable<T>
    {
    }

    public abstract class Sequence<T> : ISequence<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class RegularSequence<T> : Sequence<T>
    {
        private readonly T _value;

        public RegularSequence(T value) => _value = value;

        public override IEnumerator<T> GetEnumerator()
        {
            while (true) yield return _value;
            // ReSharper disable once IteratorNeverReturns - this is by design
        }
    }

    public class RandomSequence : Sequence<int>
    {
        private readonly Func<int> _next;

        public RandomSequence(int min, int max)
        {
            var random = new Random((int)DateTime.UtcNow.Ticks);
            _next = () => random.Next(min, max);
        }

        public override IEnumerator<int> GetEnumerator()
        {
            while (true) yield return _next();
            // ReSharper disable once IteratorNeverReturns - this is by design
        }
    }
}