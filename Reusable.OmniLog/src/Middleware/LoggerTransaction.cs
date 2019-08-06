using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    public class LoggerTransaction : LoggerMiddleware, ILoggerScope<LoggerTransaction.Scope, object>
    {
        private readonly Queue<LogEntry> _buffer = new Queue<LogEntry>();

        public LoggerTransaction() : base(false) { }

        public override bool IsActive => !(LoggerScope<Scope>.Current is null);

        protected override void InvokeCore(LogEntry request)
        {
            _buffer.Enqueue(request);
            // Don't call Next until Commit.
        }

        public Scope Push(object parameter)
        {
            return LoggerScope<Scope>.Push(new Scope { Next = Next }).Value;
        }

        public class Scope : IDisposable
        {
            private readonly Queue<LogEntry> _buffer = new Queue<LogEntry>();

            internal LoggerMiddleware Next { get; set; }

            public void Commit()
            {
                while (_buffer.Any())
                {
                    Next?.Invoke(_buffer.Dequeue());
                }
            }

            public void Rollback()
            {
                _buffer.Clear();
            }


            public void Dispose()
            {
                _buffer.Clear();
                LoggerScope<Scope>.Current.Dispose();
            }
        }
    }
}