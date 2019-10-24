using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.Flexo
{
    public interface ISwitchable
    {
        [DefaultValue(true)]
        bool Enabled { get; }
    }

    [UsedImplicitly]
    [PublicAPI]
    public interface IExpression : ISwitchable
    {
        [NotNull]
        SoftString Name { get; }

        [CanBeNull]
        string Description { get; }

        ISet<SoftString> Tags { get; }

        [CanBeNull]
        IExpression Extension { get; }

        [NotNull]
        IConstant Invoke();
    }

    public interface IExtension
    {
        /// <summary>
        /// Gets the value passed to this extension from outer context.
        /// </summary>
        object ThisOuter { get; }

        /// <summary>
        /// Indicates whether the extension is used as such or overrides it with its native value.
        /// </summary>
        bool IsInExtensionMode { get; }

        /// <summary>
        /// Gets the type the extension extends.
        /// </summary>
        Type ExtensionType { get; }
    }

    public abstract class Expression : IExpression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] BuiltInTypes =
        {
            typeof(Reusable.Flexo.IsEqual),
            typeof(Reusable.Flexo.IsNull),
            typeof(Reusable.Flexo.IsNullOrEmpty),
            typeof(Reusable.Flexo.IsGreaterThan),
            typeof(Reusable.Flexo.IsGreaterThanOrEqual),
            typeof(Reusable.Flexo.IsLessThan),
            typeof(Reusable.Flexo.IsLessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.ToDouble),
            typeof(Reusable.Flexo.ToString),
            typeof(Reusable.Flexo.GetSingle),
            typeof(Reusable.Flexo.GetMany),
            //typeof(Reusable.Flexo.SetSingle),
            //typeof(Reusable.Flexo.SetMany),
            typeof(Reusable.Flexo.Ref),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.In),
            typeof(Reusable.Flexo.Overlaps),
            typeof(Reusable.Flexo.IsSuperset),
            typeof(Reusable.Flexo.IsSubset),
            typeof(Reusable.Flexo.Matches),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Count),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.Constant<>),
            typeof(Reusable.Flexo.Double),
            typeof(Reusable.Flexo.Integer),
            typeof(Reusable.Flexo.Decimal),
            //typeof(Reusable.Flexo.DateTime),
            //typeof(Reusable.Flexo.TimeSpan),
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
            typeof(Reusable.Flexo.Collection),
            typeof(Reusable.Flexo.Select),
            typeof(Reusable.Flexo.Throw),
            typeof(Reusable.Flexo.Where),
            //typeof(Reusable.Flexo.Concat),
            //typeof(Reusable.Flexo.Union),
            typeof(Reusable.Flexo.Block),
            typeof(Reusable.Flexo.ForEach),
            typeof(Reusable.Flexo.Item),
        };
        // ReSharper restore RedundantNameQualifier

        //public static readonly Func<IImmutableContainer, IImmutableContainer> 

        private SoftString _name;

        protected Expression([NotNull] ILogger logger, SoftString name)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [NotNull]
        protected ILogger Logger { get; }

        [NotNull]
        public static ExpressionScope Scope => ExpressionScope.Current ?? throw new InvalidOperationException("Expressions must be invoked within a valid scope. Use 'BeginScope' to introduce one.");

        public virtual SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public string Description { get; set; }

        public ISet<SoftString> Tags { get; set; }

        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public IExpression Extension { get; set; }

        #region Extension aliases

        [Obsolete("Use Pipe")]
        [JsonProperty("This")]
        public IExpression ExtensionThis
        {
            get => Extension;
            set => Extension = value;
        }

        [JsonProperty("Pipe")]
        public IExpression ExtensionPipe
        {
            get => Extension;
            set => Extension = value;
        }

        #endregion

        public abstract IConstant Invoke();

        public static ExpressionScope BeginScope(Func<IImmutableContainer, IImmutableContainer> configureContext)
        {
            // Use either current context or the default one.
            return ExpressionScope.Push(configureContext(ExpressionScope.Current?.Context ?? ExpressionContext.Default));
        }

        protected class LoggerDummy : ILogger
        {
            public static LoggerDummy Instance { get; } = new LoggerDummy();
            
            public LoggerNode Node { get; }

            public T Use<T>(T next) where T : LoggerNode
            {
                return default;
            }

            public void Log(LogEntry logEntry) { }
        }
    }
}