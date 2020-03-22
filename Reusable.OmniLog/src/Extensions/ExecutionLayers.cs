using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    [PublicAPI]
    public static class ExecutionLayers
    {
        public static Action<ILogEntry> Service(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Telemetry(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> IO(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Database(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Network(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Business(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Presentation(this Action<ILogEntry> node) => node.Layer();
        
        private static Action<ILogEntry> Layer(this Action<ILogEntry> node, [CallerMemberName] string? name = null)
        {
            return node.Then(e => e.Push(new LogProperty(nameof(Layer), name!, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }
    }
}