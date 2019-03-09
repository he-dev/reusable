using System.Collections;
using System.Collections.Generic;
using Reusable.Data;

namespace Reusable
{
    public interface ICounter : IEnumerator<ICounter>
    {
        /// <summary>
        /// Gets the range of the counter.
        /// </summary>
        Range<int> Range { get; }

        /// <summary>
        /// Gets the total length of the counter.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the current value of the counter.
        /// </summary>
        int Value { get; }

        /// <summary>
        /// Gets the relative position of the counter between min and max.
        /// </summary>
        CounterPosition Position { get; }
    }

    public class Counter : ICounter
    {
        private int _value;

        public Counter(Range<int> range)
        {
            Range = range;
            Reset();
        }

        public Counter(int max) : this(new Range<int>(0, max)) { }

        public Range<int> Range { get; }

        public int Length => Range.Max - Range.Min;

        public int Value => _value + Range.Min;

        ICounter IEnumerator<ICounter>.Current => this;

        object IEnumerator.Current => this;

        public CounterPosition Position =>
            _value == 0
                ? CounterPosition.First
                : _value == Length - 1
                    ? CounterPosition.Last
                    : CounterPosition.Intermediate;

        public virtual bool MoveNext()
        {
            if (Position == CounterPosition.Last)
            {
                return false;
            }

            _value++;

            return true;
        }

        public void Reset()
        {
            _value = -1;
        }

        public void Dispose()
        {
            // There is nothing to dispose.
        }
    }

    public class InfiniteCounter : Counter
    {
        public InfiniteCounter(Range<int> range) : base(range) { }

        public InfiniteCounter(int max) : this(new Range<int>(0, max)) { }


        public override bool MoveNext()
        {
            if (!base.MoveNext())
            {
                Reset();
                base.MoveNext();
            }

            return true;
        }
    }

    public static class IndexMath
    {
        public static int Flip(this int value, int min, int max)
        {
            return (-(value - max + 1) % (max - min)) + min;
        }
    }

    public static class CounterExtensions
    {
        public static IEnumerable<ICounter> AsEnumerable(this IEnumerator<ICounter> counter)
        {
            while (counter.MoveNext())
            {
                yield return counter.Current;
            }
        }

        public static int ValueBackwards(this ICounter counter)
        {
            return counter.Value.Flip(counter.Range.Min, counter.Range.Max);
        }

        public static IEnumerator<ICounter> Infinite(this ICounter counter)
        {
            while (true)
            {
                foreach (var item in counter.AsEnumerable())
                {
                    yield return item.Current;
                }

                counter.Reset();
            }
        }
    }

    public enum CounterPosition
    {
        First,
        Intermediate,
        Last,
    }
}