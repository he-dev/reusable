using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog.Targets;
using NLog.Config;

namespace Reusable.Logging.NLogExtensions
{
    public static class LoggingConfigurationExtensions
    {
        public static IEnumerable<DatabaseTarget> DatabaseTargets(this LoggingConfiguration loggingConfiguration, Func<DatabaseTarget, bool> predicate)
        {
            return loggingConfiguration.AllTargets.OfType<DatabaseTarget>().Where(predicate);
        }
    }
}
