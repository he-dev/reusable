using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class RegularSequence<T> : GeneratedSequence<T>
    {
        private readonly T _value;
        public RegularSequence(T value, int count) : base(count) { _value = value; }
        protected override IEnumerable<T> Generate()
        {
            while (true) yield return _value;
        }
    }
}
