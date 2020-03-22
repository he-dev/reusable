using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    [UsedImplicitly]
    public class CollectScopeTelemetry : LoggerNode
    {
        private static AsyncScope<Exception>? Exception => AsyncScope<Exception>.Current;
        private static AsyncScope<Action>? LogWorkItemEnd => AsyncScope<Action>.Current;

        //private Action LogWorkItemEnd { get; set; } = () => { };

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

                AsyncScope<Action>.Push(() => LogWorkItem(propertyCopies.ToList(), snapshotName));
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
            else
            {
                request.Push(Names.Properties.Snapshot, new { status = WorkItemStatus.Begin }, m => m.ProcessWith<SerializeProperty>());
            }
        }

        private void LogWorkItem(IEnumerable<LogProperty> properties, LogProperty snapshotName)
        {
            try
            {
                var logger = ((ILoggerNode)this).First().Node<Logger>();
                var exception = Exception?.Value;
                var status = GetStatus(exception); // Sets work-item status and prevents recursive call of this node.
                logger.Log(properties, Snapshot.Take(snapshotName.Value.ToString(), new { status }), GetLogLevel(exception), exception!, status);
            }
            finally
            {
                Exception?.Dispose();
            }
        }

        private static bool IsWorkItemBegin(ILogEntry request)
        {
            // Work-item-begin is only when there is no status and category matches.
            return
                request.GetValueOrDefault(nameof(WorkItemStatus), WorkItemStatus.Undefined) == WorkItemStatus.Undefined &&
                request.TryGetProperty(Names.Properties.Category, out var category) &&
                category.Value.ToString().SoftIn(Names.Categories.WorkItem, Names.Categories.Routine);
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
            foreach (var logWorkItemEnd in LogWorkItemEnd.Enumerate())
            {
                using (logWorkItemEnd)
                {
                    logWorkItemEnd.Value();
                }
            }

            base.Dispose();
        }
    }

    public enum WorkItemStatus
    {
        Undefined,
        Begin,
        Completed,
        Canceled,
        Faulted
    }
}