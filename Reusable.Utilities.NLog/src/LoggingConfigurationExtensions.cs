using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Config;
using NLog.Targets;

namespace Reusable.Utilities.NLog
{
    public static class LoggingConfigurationExtensions
    {
        public static IEnumerable<DatabaseTarget> DatabaseTargets(this LoggingConfiguration loggingConfiguration, Func<DatabaseTarget, bool> predicate)
        {
            return loggingConfiguration.AllTargets.OfType<DatabaseTarget>().Where(predicate);
        }
    }
}
