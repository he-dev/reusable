using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class LoggerFactoryBuilder : List<ILoggerNode>
    {
        public LoggerFactoryBuilder Use<T>(LoggerFactoryBuilder builder, Action<T>? configure = default) where T : ILoggerNode, new()
        {
            Add(new T().Pipe(configure));
            return this;
        }

        public ILoggerFactory Build()
        {
            return new LoggerFactory(this);
        }
    }
}