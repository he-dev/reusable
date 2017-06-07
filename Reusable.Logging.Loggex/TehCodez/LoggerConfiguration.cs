using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging
{
    public class LoggexConfiguration
    {
        public Dictionary<string, HashSet<LogLevel>> DisabledLogLevels { get; } = new Dictionary<string, HashSet<LogLevel>>();

        public Dictionary<string, LogLevel> LogLevels { get; } = new Dictionary<string, LogLevel>();

        public Dictionary<string, object> Constants { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Environment.NewLine)] = Environment.NewLine,
            ["Tab"] = "\t"
        };

        public LoggexConfiguration()
        {

        }
    }
}
