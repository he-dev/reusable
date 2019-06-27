using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Deception
{
    public interface IPhantomException
    {
        void Throw(string name, string message = default);
    }

    [UsedImplicitly]
    public class PhantomException : IPhantomException, IEnumerable<IPhantomExceptionPattern>
    {
        private readonly IList<IPhantomExceptionPattern> _patterns = new List<IPhantomExceptionPattern>();

        public void Throw(string name, string message = default)
        {
            var matches = _patterns.Where(t => t.Matches(name)).Join(", ");
            if (matches.Any())
            {
                throw DynamicException.Create
                (
                    name ?? "Phantom",
                    message ?? $"This phantom exception was thrown because it matches [{matches}]."
                );
            }
        }

        public void Add(IPhantomExceptionPattern pattern) => _patterns.Add(pattern);

        public IEnumerator<IPhantomExceptionPattern> GetEnumerator() => _patterns.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_patterns).GetEnumerator();
    }

    /// <summary>
    /// This implementation does nothing and supports the null-object-pattern.
    /// </summary>
    public class NullPhantomException : IPhantomException
    {
        public void Throw(string name, string message = default)
        {
            // Does nothing.
        }
    }
}