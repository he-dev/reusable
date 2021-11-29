using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Reusable.Deception.Patterns
{
    public class LambdaPattern : IPhantomExceptionPattern
    {
        private readonly Func<string, IEnumerable<string>, bool> _canThrow;
        private readonly string _canThrowString;

        public LambdaPattern(Expression<Func<string, IEnumerable<string>, bool>> canThrow)
        {
            _canThrow = canThrow.Compile();
            _canThrowString = canThrow.Body.ToString();
        }

        public bool Matches(string name, params string[] tags)
        {
            return _canThrow(name, tags);
        }
        
        public override string ToString() => $"{nameof(LambdaPattern)}: '{_canThrowString}'.";

        public void Dispose() { }
    }
}