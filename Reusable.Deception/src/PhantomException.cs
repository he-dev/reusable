using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Deception.Abstractions;
using Reusable.Exceptionizer;

namespace Reusable.Deception
{
    public interface IPhantomException
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

    [UsedImplicitly]
    public class PhantomException : IPhantomException
    {
        private readonly IEnumerable<IPhantomExceptionTrigger> _triggers;

        public PhantomException(IEnumerable<IPhantomExceptionTrigger> triggers)
        {
            _triggers = triggers;
        }

        // Caches trap keys because creating them requires reflection via StackTrace and we don't want to create a bottleneck.
        private readonly ConcurrentDictionary<string, IList<StackFrameInfo>> _traps = new ConcurrentDictionary<string, IList<StackFrameInfo>>();

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
            var names = _traps.GetOrAdd(key, _ =>
            {
                // We want the 3rd frame because they go like this: GetOrAdd, Throw, Caller
                //var method = new StackTrace().GetFrame(3).GetMethod();
                // Well... this was pretty unreliable. Just get the entire stack and try to match the filter in any frame.

                return new StackTrace().GetFrames()?.Select(f => new StackFrameInfo(f, id)).ToList();                
            });

            var localTestResult = test?.Invoke() ?? true;
            var trigger = localTestResult ? _triggers.FirstOrDefault(x => x.CanThrow(names)) : default;
            if (!(trigger is null))
            {
                throw DynamicException.Create
                (
                    trigger.Exception ?? "Phantom",
                    $"This is a phantom exception triggered by the '{trigger}' that matched [{names.Join(", ")}]. {trigger.Message}"
                );
            }
        }
    }

    public class StackFrameInfo : IEnumerable<string>
    {
        public StackFrameInfo(StackFrame stackFrame, string id)
        {
            var method = stackFrame?.GetMethod();
            Namespace = method?.DeclaringType?.Namespace;
            Type = method?.DeclaringType?.Name;
            Method = method?.Name;
            Id = id;
        }

        public string Namespace { get;  }

        public string Type { get; }

        public string Method { get; }

        public string Id { get; }

        public IEnumerator<string> GetEnumerator()
        {
            yield return Namespace;
            yield return Type;
            yield return Method;
            yield return Id;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    } 
}