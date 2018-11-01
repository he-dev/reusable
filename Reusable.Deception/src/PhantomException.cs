using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Diagnostics.Abstractions;
using Reusable.Diagnostics.Triggers;
using Reusable.Reflection;

namespace Reusable.Diagnostics
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
        private readonly ConcurrentDictionary<string, IList<string>> _traps = new ConcurrentDictionary<string, IList<string>>();

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
                var method = new StackTrace().GetFrame(3).GetMethod();

                // ReSharper disable once PossibleNullReferenceException - I doubt 'DeclaringType' is ever 'null'.
                return new[]
                {
                    method.DeclaringType.Namespace,
                    method.DeclaringType.Name,
                    method.Name,
                    id
                };
            });

            var localTestResult = test?.Invoke() ?? true;
            var trigger = localTestResult ? _triggers.FirstOrDefault(x => x.CanThrow(names)) : default;
            if (!(trigger is null))
            {
                throw DynamicException.Create(trigger.Exception ?? "Phantom", $"This is a phantom exception triggered by the '{trigger}' that matched [{names.Join(", ")}]. {trigger.Message}");
            }
        }
    }
}