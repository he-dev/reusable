using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Utilities;
using Reusable.OmniLog.Utilities.Logging;

namespace Reusable.OmniLog.Nodes
{
    [UsedImplicitly]
    public class CollectFlowTelemetry : LoggerNode
    {
        private static AsyncScope<FlowData>? CurrentFlowData => AsyncScope<FlowData>.Current;
        //private static AsyncScope<FlowEnd>? CurrentFlowEnd => AsyncScope<FlowEnd>.Current;

        public HashSet<string> Categories { get; set; } = new HashSet<string>
        {
            Names.Categories.WorkItem,
            Names.Categories.Routine
        };

        public HashSet<string> CopyProperties { get; set; } = new HashSet<string>
        {
            Names.Properties.Layer,
            Names.Properties.Category
        };

        // ReSharper disable once MemberCanBeMadeStatic.Global - this should remain instance method so that it must be accessed via logger-scope
        public void Push(Exception? exception)
        {
            //if(CurrentFlowEnd is null) throw new InvalidOperationException($"Cannot push exception because there is no valid flow scope.");
            //InjectFlowScope.Current.Value.DeferredWorkItems.Push();
            AsyncScope<FlowData>.Push(new FlowData { Exception = exception });
        }

        public override void Invoke(ILogEntry request)
        {
            if (IsFlowBegin(request))
            {
                UpdateSnapshot(request);

                // Use the same snapshot-name later.
                request.TryGetProperty(Names.Properties.Unit, out var unit);

                var propertyCopies =
                    from propertyName in CopyProperties
                    let property = request[propertyName]
                    select property;

                ToggleScope.Current!.Value.DeferredWorkItems.Push(new FlowEnd { Log = () => LogWorkItem(propertyCopies.ToList(), unit) });
            }

            InvokeNext(request);
        }

        private static void UpdateSnapshot(ILogEntry request)
        {
            // Add work-item-status to the snapshot.
            var data = CreateFlowData(FlowStatus.Begin, request.GetValueOrDefault(Names.Properties.Snapshot, default(object?)));
            request.Push(Names.Properties.Snapshot, data, m => m.ProcessWith<SerializeProperty>());
        }

        private static object CreateFlowData(FlowStatus status, object? value)
        {
            return value switch
            {
                {} => new { status, value },
                null => new { status }
            };
        }

        private void LogWorkItem(IEnumerable<LogProperty> properties, LogProperty unit)
        {
            try
            {
                var logger = ((ILoggerNode)this).First().Node<Logger>();
                var flowData = CurrentFlowData?.Value;
                var status = GetFlowStatus(flowData?.Exception); // Sets flow status and prevents recursive call of this node.
                logger.Log(properties, Snapshot.Take(unit.Value.ToString(), new { status }), GetLogLevel(flowData?.Exception), flowData?.Exception!, status);
            }
            finally
            {
                CurrentFlowData?.Dispose();
            }
        }

        private bool IsFlowBegin(ILogEntry request)
        {
            // Flow-begin is only when there is no status and category matches.
            return
                request.GetValueOrDefault(nameof(FlowStatus), FlowStatus.Undefined) == FlowStatus.Undefined &&
                request.TryGetProperty(Names.Properties.Category, out var category) &&
                category.Value.ToString().SoftIn(Categories);
        }

        private static FlowStatus GetFlowStatus(Exception? exception)
        {
            return exception switch
            {
                null => FlowStatus.Completed,
                OperationCanceledException _ => FlowStatus.Canceled,
                {} => FlowStatus.Faulted
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

        // public override void Dispose()
        // {
        //     if (CurrentFlowEnd is {} flowEnd)
        //     {
        //         using (flowEnd)
        //         {
        //             flowEnd.Value.Log();
        //         }
        //     }
        //
        //     base.Dispose();
        // }

        private class FlowEnd : IDisposable
        {
            public Action Log { get; set; } = default!;
            
            public void Dispose() => Log();
        }

        private class FlowData
        {
            public Exception? Exception { get; set; }
        }
    }

    

    public class LogBeginScope : LoggerNode
    {
        public override void Invoke(ILogEntry request)
        {
            throw new NotImplementedException();
        }
    }
    
    public class LogEndScope : LoggerNode
    {
        public override void Invoke(ILogEntry request)
        {
            throw new NotImplementedException();
        }
    }
}