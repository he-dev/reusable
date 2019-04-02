using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using Reusable.Collections;
using Reusable.Extensions;

namespace Reusable.OmniLog
{
    public static class LogRenderer
    {
        internal static Log Render(this Log log, IEnumerable<ILogAttachment> attachments)
        {
            // Recreate the log with all properties and compute them.

            // Get get last added attachment. This allows us to override them within a scope.
            attachments =
                attachments
                    .Concat(LogScope.Attachments())
                    .GroupBy(a => a.Name)
                    .Select(g => g.Last());

            var attachmentProperties = attachments.Select(attachment => new KeyValuePair<SoftString, object>(attachment.Name, attachment));
            var properties = log.Concat(attachmentProperties);

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

                    // It is allowed to set the value before the attachment is computed.
                    case ILogAttachment attachment:
                        log[attachment.Name] = attachment.Compute(rawLog);
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