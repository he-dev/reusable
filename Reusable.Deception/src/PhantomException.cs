using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Deception
{
    public interface IPhantomException
    {
        void Throw(string name);
    }

    [UsedImplicitly]
    public class PhantomException : IPhantomException
    {
        private readonly IEnumerable<IPhantomExceptionTrigger> _triggers;

        public PhantomException(IEnumerable<IPhantomExceptionTrigger> triggers)
        {
            _triggers = triggers;
        }
        
        // Prevent inlining just in case so that the stack-trace does not change.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Throw(string name)
        {
            foreach (var trigger in _triggers.Where(t => t.CanThrow(name)))
            {
                throw DynamicException.Create
                (
                    trigger.Exception ?? "Phantom",
                    $"This is a phantom exception triggered by the '{trigger}' that matched []. {trigger.Message}"
                );
            }
        }
    }

    /// <summary>
    /// This implementation does nothing and support the null-object-pattern.
    /// </summary>
    public class NullPhantomException : IPhantomException
    {
        public void Throw(string name)
        {
            // Does nothing.
        }
    }
}