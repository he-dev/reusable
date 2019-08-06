using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class TransactionNode : LoggerNode, ILoggerScope<TransactionNode.Scope, object>
    {
        public TransactionNode() : base(false) { }

        public override bool IsActive => !LoggerScope<Scope>.IsEmpty;

        protected override void InvokeCore(LogEntry request)
        {
            LoggerScope<Scope>.Current.Value.Buffer.Enqueue(request);
            // Don't call Next until Commit.
        }

        public Scope Push(object parameter)
        {
            return LoggerScope<Scope>.Push(new Scope { Next = Next }).Value;
        }

        public class Scope : IDisposable
        {
            internal Queue<LogEntry> Buffer { get; } = new Queue<LogEntry>();

            internal LoggerNode Next { get; set; }

            public void Commit()
            {
                while (Buffer.Any())
                {
                    Next?.Invoke(Buffer.Dequeue());
                }
            }

            public void Rollback()
            {
                Buffer.Clear();
            }


            public void Dispose()
            {
                Buffer.Clear();
                LoggerScope<Scope>.Current.Dispose();
            }
        }
    }
}