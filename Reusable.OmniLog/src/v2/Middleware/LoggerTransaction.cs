using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerTransaction : LoggerMiddleware
    {
        private readonly Queue<ILog> _buffer = new Queue<ILog>();

        public LoggerTransaction()
        {
            IsActive = false;
        }
        
        protected override void InvokeCore(ILog request)
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