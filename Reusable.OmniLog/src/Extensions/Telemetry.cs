using System;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Data;

namespace Reusable.OmniLog.Extensions
{
    public delegate void Link<in T>(ILogEntry log, T from);

    public interface ITelemetry { }

    // Layers

    public interface ITelemetryLayer { }

    // Categories

    public interface ITelemetryCategory { }

    public interface ITelemetryCategoryDependency { }

    public interface ITelemetryCategoryProcess { }

    public interface ITelemetryCategoryLogic { }

    public interface ITelemetryCategoryEvent { }

    // Units

    public interface ITelemetryUnit { }

    // Extensions

    public interface IDecision { }

    public static class Telemetry
    {
        public static Link<ITelemetry?> Collect => (log, _) => { };
    }

    public static class TelemetryLayers
    {
        public static Link<ITelemetryCategoryDependency?> Dependency(this Link<ITelemetry?> telemetry) => (log, _) => telemetry(log, default);

        public static Link<ITelemetryLayer?> File(this Link<ITelemetryCategoryDependency?> telemetry) => (log, _) => telemetry(log.Layer(nameof(File)), default);
        public static Link<ITelemetryLayer?> Directory(this Link<ITelemetryCategoryDependency?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Directory)), default);
        public static Link<ITelemetryLayer?> Http(this Link<ITelemetryCategoryDependency?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Http)), default);
        public static Link<ITelemetryLayer?> Database(this Link<ITelemetryCategoryDependency?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Database)), default);
        public static Link<ITelemetryLayer?> Resource(this Link<ITelemetryCategoryDependency?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Resource)), default);
        public static Link<ITelemetryLayer?> Network(this Link<ITelemetryCategoryDependency?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Network)), default);

        public static Link<ITelemetryLayer?> Application(this Link<ITelemetry?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Application)), default);
        public static Link<ITelemetryLayer?> Business(this Link<ITelemetry?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Business)), default);
        public static Link<ITelemetryLayer?> Presentation(this Link<ITelemetry?> telemetry) => (log, _) => telemetry(log.Layer(nameof(Presentation)), default);
    }

    public static class TelemetryCategories
    {
        public static Link<ITelemetryCategoryProcess?> Task(this Link<ITelemetryLayer?> telemetry, string name) => (log, _) => telemetry(log.Category(nameof(Task)).Unit(name), default);
        public static Link<ITelemetryCategoryLogic?> Logic(this Link<ITelemetryLayer?> telemetry) => (log, _) => telemetry(log.Category(nameof(Logic)), default);

        public static Action<ILogEntry> Metric(this Link<ITelemetryLayer?> telemetry, string name, double value) => (log) => telemetry(log.Category(nameof(Metric)).Unit(name, value), default);
        public static Action<ILogEntry> Metric(this Link<ITelemetryLayer?> telemetry, string name, string value) => (log) => telemetry(log.Category(nameof(Metric)).Unit(name, value), default);

        public static Action<ILogEntry> Argument(this Link<ITelemetryLayer?> telemetry, string name, object value) => (log) => telemetry(log.Category(nameof(Argument)).Unit(name, value), default);
        public static Action<ILogEntry> Variable(this Link<ITelemetryLayer?> telemetry, string name, object value) => (log) => telemetry(log.Category(nameof(Variable)).Unit(name, value), default);
        public static Action<ILogEntry> Property(this Link<ITelemetryLayer?> telemetry, string name, object value) => (log) => telemetry(log.Category(nameof(Property)).Unit(name, value), default);
        public static Action<ILogEntry> Metadata(this Link<ITelemetryLayer?> telemetry, string name, object value) => (log) => telemetry(log.Category(nameof(Metadata)).Unit(name, value), default);
        public static Action<ILogEntry> WorkItem(this Link<ITelemetryLayer?> telemetry, string name, object value) => (log) => telemetry(log.Category(nameof(WorkItem)).Unit(name, value), default);
    }

    public static class TelemetryUnits
    {
        public static Action<ILogEntry> Unit(this Link<ITelemetryUnit?> telemetry, string name, object? value = default) => (log) => telemetry(log.Unit(name, value), default);

        public static Action<ILogEntry> Status(this Link<ITelemetryCategoryProcess?> telemetry, FlowStatus status, object? workItem = default) => (log) => telemetry(log.Snapshot(new { status, workItem }), default);
        
        public static Link<IDecision> Decision(this Link<ITelemetryCategoryLogic?> telemetry, string decision) => (log, _) => telemetry(log.Unit(nameof(Decision), decision), default);
        public static Action<ILogEntry> Because(this Link<IDecision?> telemetry, string reason) => (log) => telemetry(log.Message(reason), default);
    }

    public static class TelemetryPopular
    {
        public static Action<ILogEntry> Message(this Action<ILogEntry> node, string? message) => node.Then(e => e.Message(message));

        public static Action<ILogEntry> Exception(this Action<ILogEntry> node, Exception? exception) => node.Then(e => e.Exception(exception));
    }

    public static class TelemetryLevels
    {
        public static Action<ILogEntry> Level(this Action<ILogEntry> node, LogLevel logLevel) => node.Then(e => e.Level(logLevel));

        public static Action<ILogEntry> Trace(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Debug(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Warning(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Information(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Error(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Fatal(this Action<ILogEntry> node) => node.Level();

        private static Action<ILogEntry> Level(this Action<ILogEntry> node, [CallerMemberName] string? name = null) => node.Level((LogLevel)Enum.Parse(typeof(LogLevel), name));
    }
}