using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions.Data
{
    [PublicAPI]
    public readonly struct LogProperty
    {
        public LogProperty(SoftString name, object? value, ILogPropertyAction state)
        {
            Name = name;
            Value = value;
            Action = state;
        }

        public SoftString Name { get; }

        public object? Value { get; }

        public ILogPropertyAction Action { get; }

        public bool IsEmpty => Value is null && Action is null;
    }

    public static class LogPropertyExtensions
    {
        public static T ValueOrDefault<T>(this LogProperty property, T defaultValue = default)
        {
            return property.Value switch { T t => t, _ => defaultValue };
        }
    }

    public interface ILogPropertyAction { }

    namespace LogPropertyActions
    {
        /// <summary>
        /// Item is suitable for logging.
        /// </summary>
        public readonly struct Log : ILogPropertyAction { }

        /// <summary>
        /// Item needs to be exploded.
        /// </summary>
        public readonly struct Destructure : ILogPropertyAction { }

        /// <summary>
        /// Item needs to be mapped or serialized.
        /// </summary>
        public readonly struct Serialize : ILogPropertyAction { }

        public readonly struct Copy : ILogPropertyAction { }

        public readonly struct Delete : ILogPropertyAction { }

        public readonly struct Build : ILogPropertyAction { }
    }
}