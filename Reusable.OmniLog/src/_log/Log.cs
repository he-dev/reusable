using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class Log : Dictionary<SoftString, object>, ILog
    {
        private readonly IDictionary<SoftString, object> _data = new Dictionary<SoftString, object>();

        public Log() { }

        public Log(IDictionary<SoftString, object> source) : base(source) { }

        public static ILog Empty => new Log();

        public Log SetItem(string name, object value)
        {
            if (value == Log.PropertyNames.Unset)
            {
                Remove(name);
            }
            else
            {
                this[name] = value;
            }

            return this;
        }

        /// <summary>
        /// Provides default property names.
        /// </summary>
        public static class PropertyNames
        {
            public static readonly string Logger = nameof(Logger);
            public static readonly string Category = nameof(Category);
            public static readonly string Level = nameof(Level);
            public static readonly string Message = nameof(Message);
            public static readonly string Exception = nameof(Exception);
            public static readonly string Elapsed = nameof(Elapsed);
            public static readonly string Timestamp = nameof(Timestamp);
            public static readonly string CallerMemberName = nameof(CallerMemberName);
            public static readonly string CallerLineNumber = nameof(CallerLineNumber);
            public static readonly string CallerFilePath = nameof(CallerFilePath);
            public static readonly string OverridesTransaction = nameof(OverridesTransaction);

            //public static readonly SoftString Scope = nameof(Scope);
            //public static readonly SoftString CorrelationId = nameof(CorrelationId);
            //public static readonly SoftString Context = nameof(Context);

            // This field can be used to remove a property from log.
            public static readonly object Unset = new object();
        }
    }
}