using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Sequences
{
    [PublicAPI]
    public class HarmonicSequence<T> : Sequence<T>
    {
        private readonly T _one;
        private readonly Func<T, T, T> _add;
        private readonly Func<T, T, T> _divide;

        public HarmonicSequence(T one)
        {            
            _one = one;

            var leftParameter = Expression.Parameter(typeof(T), "left");
            var rightParameter = Expression.Parameter(typeof(T), "right");
            var divide = Expression.Divide(leftParameter, rightParameter);
            _divide = Expression.Lambda<Func<T, T, T>>(divide, leftParameter, rightParameter).Compile();

            var add = Expression.Add(leftParameter, rightParameter);
            _divide = Expression.Lambda<Func<T, T, T>>(divide, leftParameter, rightParameter).Compile();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            var current = _one;
            while (true)
            {
                yield return current;
                current = _divide(_one, current);
            }
        }
    }    
}
