using System;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogRx
    {
        void Log(Log log);
    }
}