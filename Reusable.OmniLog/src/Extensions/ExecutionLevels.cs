using System;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class ExecutionLevels
    {
        public static Action<ILogEntry> Trace(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Debug(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Warning(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Information(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Error(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Fatal(this Action<ILogEntry> node) => node.Level();

        public static Action<ILogEntry> Level(this Action<ILogEntry> node, LogLevel logLevel)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Properties.Level, logLevel, LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }
        
        private static Action<ILogEntry> Level(this Action<ILogEntry> node, [CallerMemberName] string? name = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return node.Level((LogLevel)Enum.Parse(typeof(LogLevel), name));
        }
    }
}