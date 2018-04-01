using System;
using System.Collections.Generic;
using System.Reactive;
using Reusable.OmniLog.Collections;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class MemoryRx : LogRx
    {
        private readonly List<Log> _logs = new List<Log>();

        public IReadOnlyList<Log> Logs => _logs;

        protected override IObserver<Log> Initialize()
        {
            return Observer.Create<Log>(log => _logs.Add(log));
        }

        public static MemoryRx Create() => new MemoryRx();
    }
}