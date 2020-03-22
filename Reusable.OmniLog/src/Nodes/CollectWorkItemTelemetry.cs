using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    [UsedImplicitly]
    public class CollectWorkItemTelemetry : LoggerNode
    {
        private static AsyncScope<Exception>? Context => AsyncScope<Exception>.Current;

        private Action LogWorkItemEnd { get; set; } = () => { };

        public HashSet<string> CopyProperties { get; set; } = new HashSet<string>
        {
            Names.Properties.Layer,
            Names.Properties.Category
        };

        // ReSharper disable once MemberCanBeMadeStatic.Global - this should remain instance method so that it must be accessed via logger-scope
        public void Push(Exception exception) => AsyncScope<Exception>.Push(exception);

        public override void Invoke(ILogEntry request)
        {
            if (IsWorkItemBegin(request))
            {
                UpdateWorkItem(request);

                // Use the same snapshot-name later.
                request.TryGetProperty(Names.Properties.SnapshotName, out var snapshotName);

                var propertyCopies =
                    from propertyName in CopyProperties
                    let property = request[propertyName]
                    select property;

                LogWorkItemEnd = () => LogWorkItem(propertyCopies.ToList(), snapshotName);
            }

            InvokeNext(request);
        }

        private static void UpdateWorkItem(ILogEntry request)
        {
            // Add work-item-status to the snapshot.
            if (request.TryGetProperty(Names.Properties.Snapshot, out var snapshot))
            {
                request.Push(Names.Properties.Snapshot, new { status = WorkItemStatus.Begin, value = snapshot.Value }, m => m.ProcessWith<SerializeProperty>());
            }
        }

        private void LogWorkItem(IEnumerable<LogProperty> properties, LogProperty snapshotName)
        {
            try
            {
                Enabled = false;
                var logger = ((ILoggerNode)this).First().Node<Logger>();
                var exception = Context?.Value;
                logger.Log(properties, Snapshot.Take(snapshotName.Value.ToString(), new { status = GetStatus(exception) }), GetLogLevel(exception), exception!);
            }
            finally
            {
                Enabled = true;
                Context?.Dispose();
            }
        }

        private static bool IsWorkItemBegin(ILogEntry request)
        {
            return request.TryGetProperty(Names.Properties.Category, out var category) && SoftString.Comparer.Equals(category.Value.ToString(), Names.Categories.WorkItem);
        }

        private static WorkItemStatus GetStatus(Exception? exception)
        {
            return exception switch
            {
                null => WorkItemStatus.Completed,
                OperationCanceledException _ => WorkItemStatus.Canceled,
                {} => WorkItemStatus.Faulted
            };
        }

        private static LogLevel GetLogLevel(Exception? exception)
        {
            return exception switch
            {
                null => LogLevel.Information,
                OperationCanceledException e => LogLevel.Warning,
                {} e => LogLevel.Error
            };
        }

        public override void Dispose()
        {
            LogWorkItemEnd();
            base.Dispose();
        }
    }

    public enum WorkItemStatus
    {
        Begin,
        Completed,
        Canceled,
        Faulted
    }
}