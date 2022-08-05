using System;
using System.Collections.Generic;

namespace Reusable.Marbles.Collections;

internal static class ComparerFactory<T, TValue>
{
    internal class Comparer : IComparer<T>
    {
        private readonly Func<T, TValue> _selectValue;
        private readonly Func<TValue, TValue, bool> _isLessThan;
        private readonly Func<TValue, TValue, bool> _areEqual;
        private readonly Func<TValue, TValue, bool> _isGreaterThan;

        public Comparer(Func<T, TValue> selectValue)
        {
            _selectValue = selectValue;
            _isLessThan = (x, y) => Comparer<TValue>.Default.Compare(x, y) < 0;
            _areEqual = (x, y) => Comparer<TValue>.Default.Compare(x, y) == 0;
            _isGreaterThan = (x, y) => Comparer<TValue>.Default.Compare(x, y) > 0;
        }

        public int Compare(T? x, T? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(x, null)) return -1;
            if (ReferenceEquals(y, null)) return 1;

            var (xValue, yValue) = (_selectValue(x), _selectValue(y));

            if (ReferenceEquals(xValue, yValue)) return 0;
            if (ReferenceEquals(xValue, null)) return -1;
            if (ReferenceEquals(yValue, null)) return 1;

            if (_isLessThan(xValue, yValue)) return -1;
            if (_areEqual(xValue, yValue)) return 0;
            if (_isGreaterThan(xValue, yValue)) return 1;

            // Makes the compiler very happy.
            return 0;
        }
    }
}

public static class ComparerFactory<T>
{
    public static IComparer<T> Create<TComparable>(Func<T, TComparable> selectValue)
    {
        return new ComparerFactory<T, TComparable>.Comparer(selectValue);
    }
}