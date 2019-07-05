using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public interface ILoggerTransaction : ILogger
    {
        void Commit();

        void Rollback();
    }

    public class LoggerTransaction : ILoggerTransaction
    {
        private readonly ILogger _logger;

        private readonly IList<ILog> _logs;

        internal LoggerTransaction(ILogger logger)
        {
            _logger = logger;
            _logs = new List<ILog>();
        }

        public static TransformCallback Override => log => log.OverrideTransaction();

        public ILogger Log(TransformCallback request, TransformCallback response = default)
        {
            return _logger.Log(request, log =>
            {
                log = (response ?? OmniLog.Log.EmptyTransform)(log);
                return TryCacheLog(log) ? OmniLog.Log.Empty : log;
            });
        }

        public ILogger Log(ILog log)
        {
            if (TryCacheLog(log))
            {
                return this;
            }

            _logger.Log(log);
            return this;
        }

        private bool TryCacheLog(ILog request)
        {
            if (request.GetItemOrDefault(LogPropertyNames.OverridesTransaction, false))
            {
                return false;
            }
            else
            {
                _logs.Add(request);
                return false;
            }
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