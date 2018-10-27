using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public static class LogRenderer
    {
        internal static Log Render(this Log log, IEnumerable<ILogAttachement> attachements)
        {
            // Recreate the log with all properties and compute them.

            var properties =
                log
                    .Concat(attachements.Select(attachement => new KeyValuePair<SoftString, object>(attachement.Name, attachement)));
                    //.Append(new KeyValuePair<SoftString, object>("Scopes", LogScope.Current.Flatten().Select(scope => scope).ToList()));
                    //.Concat(LogScope.Current.Flatten().Select(scope => (Key: scope.Name, Value: (object)scope)));

            log = new Log().AddRangeSafely(properties);
            return log.Compute(log);
        }

        private static Log Compute(this Log source, Log rawLog)
        {
            var log = new Log();

            foreach (var item in source)
            {
                switch (item.Value)
                {
                    // Don't render LogLevel because we cannot filter without it.
                    //	case LogLevel logLevel:
                    //		log[item.Key] = logLevel.ToString();
                    //		break;s

                    case MessageFunc messageFunc:
                        log[LogProperties.Message] = messageFunc();
                        break;

                    case ILogAttachement computable:
                        log[item.Key] = computable.Compute(rawLog);
                        break;
                    
                    default:
                        log[item.Key] = item.Value;
                        break;
                }
            }

            return log;
        }
    }
}