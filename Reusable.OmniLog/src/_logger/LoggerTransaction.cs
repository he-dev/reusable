using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public interface ILoggerTransaction : ILogger
    {
        void Commit();

        void Rollback();
        
    }
    internal class LoggerTransaction : ILoggerTransaction
    {
        private readonly ILogger _logger;
        
        private readonly IList<ILog> _logs;

        public LoggerTransaction(ILogger logger)
        {
            _logger = logger;
            _logs = new List<ILog>();
        }

        public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default)
        {
            return _logger.Log(populate, r =>
            {
                if (r.GetItemOrDefault(LogPropertyNames.OverridesTransaction, false))
                {
                    return r;
                }
                else
                {
                    _logs.Add(r);
                    return OmniLog.Log.Empty;
                }
            });
        }

        public ILogger Log(ILog log)
        {
            if (log.GetItemOrDefault(LogPropertyNames.OverridesTransaction, false))
            {
                _logger.Log(log);
            }
            else
            {
                _logs.Add(log);
            }

            return this;
        }

        public void Commit()
        {
            foreach (var log in _logs)
            {
                _logger.Log(log);
            }

            Rollback();
        }

        public void Rollback()
        {
            _logs.Clear();
        }

        public void Dispose()
        {
            // You don't want to dispose it here because it'll unwire all listeners.
            // _logger.Dispose();
            _logs.Clear();
        }
    }
}