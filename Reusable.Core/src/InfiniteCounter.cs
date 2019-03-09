using System.Collections;
using System.Collections.Generic;

namespace Reusable
{
    public interface IInfiniteCounter : IEnumerator<IInfiniteCounter>
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
        InfiniteCounterPosition Position { get; }
    }

    public class InfiniteCounter : IInfiniteCounter
    {
        private int _value;

        public InfiniteCounter(Range<int> range)
        {
            Range = range;
            Reset();
        }

        public InfiniteCounter(int max) : this(new Range<int>(0, max)) { }

        public Range<int> Range { get; }

        public int Length => Range.Max - Range.Min;

        public int Value => _value + Range.Min;

        IInfiniteCounter IEnumerator<IInfiniteCounter>.Current => this;

        object IEnumerator.Current => this;

        public InfiniteCounterPosition Position =>
            _value == 0
                ? InfiniteCounterPosition.First
                : _value == Length - 1
                    ? InfiniteCounterPosition.Last
                    : InfiniteCounterPosition.Intermediate;

        public bool MoveNext()
        {
            if (Position == InfiniteCounterPosition.Last)
            {
                Reset();
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

    public static class IndexMath
    {
        public static int Flip(this int value, int min, int max)
        {
            return (-(value - max + 1) % (max - min)) + min;
        }
    }

    public static class InfiniteCounterExtensions
    {
        public static IEnumerable<IInfiniteCounter> AsEnumerable(this IEnumerator<IInfiniteCounter> counter)
        {
            while (counter.MoveNext())
            {
                yield return counter.Current;
            }
        }

        public static int ValueAsCountdown(this IInfiniteCounter counter)
        {
            return counter.Value.Flip(counter.Range.Min, counter.Range.Max);
        }
    }

    public enum InfiniteCounterPosition
    {
        First,
        Intermediate,
        Last,
    }
}