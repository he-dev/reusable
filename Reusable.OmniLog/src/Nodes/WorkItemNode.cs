using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    [UsedImplicitly]
    public class WorkItemNode : LoggerNode
    {
        private Action LogWorkItemEnd { get; set; } = () => { };

        public Func<ILogEntry, bool> IsWorkItemBegin { get; set; } = WorkItemHelper.IsWorkItemBegin("Category", "WorkItem");

        public HashSet<string> CopyProperties { get; set; } = new HashSet<string>
        {
            "Layer",
            "Category",
        };

        public void Push(Exception exception) => AsyncScope<Exception>.Push(exception);

        public override void Invoke(ILogEntry request)
        {
            if (IsWorkItemBegin(request))
            {
                UpdateWorkItem(request);
                
                // Use the same snapshot-name later.
                request.TryGetProperty(LogProperty.Names.SnapshotName, out var snapshotName);

                var propertyCopies =
                    from propertyName in CopyProperties
                    let property = request[propertyName]
                    where property.HasValue
                    select property.Value;

                LogWorkItemEnd = () => LogWorkItem(propertyCopies.ToList(), snapshotName);
            }

            InvokeNext(request);
        }

        private static void UpdateWorkItem(ILogEntry request)
        {
            // Add work-item-status to the snapshot.
            request.TryGetProperty(LogProperty.Names.Snapshot, out var snapshot);
            ((IDictionary<string, object>)snapshot.Value).Add("status", WorkItemStatus.Begin);
        }

        private void LogWorkItem(IEnumerable<LogProperty> propertyCopies, LogProperty snapshotName)
        {
            try
            {
                Enabled = false;
                var logger = ((ILoggerNode)this).First().Node<Logger>();
                var exception = AsyncScope<Exception>.Current?.Value;
                logger.Log(propertyCopies, Snapshot.Take(snapshotName.Value.ToString(), new { status = GetStatus(exception) }), GetLogLevel(exception), exception);
            }
            finally
            {
                Enabled = true;
                AsyncScope<Exception>.Current?.Dispose();
            }
        }
        
        private static WorkItemStatus GetStatus(Exception exception)
        {
            return exception switch
            {
                null => WorkItemStatus.Completed,
                OperationCanceledException _ => WorkItemStatus.Canceled,
                {} => WorkItemStatus.Faulted
            };
        }

        private static LogLevel GetLogLevel(Exception exception)
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

    public static class WorkItemHelper
    {
        public static WorkItemNode WorkItem(this BranchNode branch) => branch.First.Node<WorkItemNode>();

        public static Func<ILogEntry, bool> IsWorkItemBegin(string propertyName, string propertyValue)
        {
            return e => e.TryGetProperty(propertyName, out var property) && SoftString.Comparer.Equals(property.Value?.ToString(), propertyValue);
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