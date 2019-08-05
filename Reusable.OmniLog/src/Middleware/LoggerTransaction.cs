using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerTransaction : LoggerMiddleware
    {
        private readonly Queue<ILog> _buffer = new Queue<ILog>();

        public LoggerTransaction() : base(false) { }

        protected override void InvokeCore(Abstractions.v2.Log request)
        {
            _buffer.Enqueue(request);
            // Don't call Next until Commit.
        }

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
    }
}