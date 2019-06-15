using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Keynetic;
using Reusable.OmniLog.Abstractions;

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

        [CanBeNull]
        IExpression Extension { get; }

        [NotNull]
        IConstant Invoke();
    }

    public interface IExtension<out T>
    {
        T This { get; }
    }

    public static class ExpressionContext
    {
        public static IImmutableSession Default =>
            ImmutableSession
                .Empty
                .SetItem(From<IExpressionMeta>.Select(m => m.Comparers), ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                .SetItem(From<IExpressionMeta>.Select(m => m.References), ImmutableDictionary<SoftString, IExpression>.Empty)
                .SetItem(From<IExpressionMeta>.Select(m => m.DebugView), TreeNode.Create(ExpressionDebugView.Root))
                .WithDefaultComparer()
                .WithSoftStringComparer()
                .WithRegexComparer();
    }

    public abstract class Expression : IExpression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
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
            typeof(Reusable.Flexo.SetSingle),
            typeof(Reusable.Flexo.SetMany),
            typeof(Reusable.Flexo.Ref),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.In),
            typeof(Reusable.Flexo.Overlaps),
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
            typeof(Reusable.Flexo.Concat),
            typeof(Reusable.Flexo.Union),
            typeof(Reusable.Flexo.Block),
            typeof(Reusable.Flexo.ForEach),
            typeof(Reusable.Flexo.Item),
        };
        // ReSharper restore RedundantNameQualifier

        private SoftString _name;

        protected Expression([NotNull] ILogger logger, SoftString name)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [NotNull]
        protected ILogger Logger { get; }

        [NotNull]
        public static ExpressionScope Scope =>
            ExpressionScope.Current
            ?? throw new InvalidOperationException("Expressions must be invoked within a valid scope. Use 'BeginScope' to introduce one.");

        //public static INamespace<IExpressionMeta> Namespace => Use<IExpressionMeta>.Namespace;

        public virtual SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public string Description { get; set; }

        public bool Enabled { get; set; } = true;

        [JsonProperty("This")]
        public IExpression Extension { get; set; }

        public abstract IConstant Invoke();

        public static ExpressionScope BeginScope(Func<IImmutableSession, IImmutableSession> configureContext)
        {
            return ExpressionScope.Push(configureContext(ExpressionScope.Current?.Context ?? ExpressionContext.Default));
        }

        protected class ZeroLogger : ILogger
        {
            private ZeroLogger() { }

            public static ILogger Default => new ZeroLogger();

            public SoftString Name { get; } = nameof(ZeroLogger);

            public ILogger Log(ILogLevel logLevel, Action<ILog> logAction) => this;

            public void Dispose() { }
        }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class ExpressionScope : IDisposable
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd conflict with the property that has the same name.
        private static readonly AsyncLocal<ExpressionScope> _current = new AsyncLocal<ExpressionScope>();

        static ExpressionScope() { }

        private ExpressionScope(int depth)
        {
            Depth = depth;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.Property(x => x.CorrelationId);
            //builder.Property(x => x.CorrelationContext);
            builder.DisplayValue(x => x.Depth);
        });

        public ExpressionScope Parent { get; private set; }

        /// <summary>
        /// Gets the current log-scope which is the deepest one.
        /// </summary>
        [CanBeNull]
        public static ExpressionScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public int Depth { get; }

        public IImmutableSession Context { get; private set; }

        public static ExpressionScope Push(IImmutableSession context)
        {
            var scope = Current = new ExpressionScope(Current?.Depth + 1 ?? 0)
            {
                Parent = Current,
                Context = context
            };
            return scope;
        }

        public void Dispose()
        {
            Current = Current?.Parent;
        }
    }

    [UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
    public interface IExpressionMeta : INamespace
    {
        /// <summary>
        /// Gets or sets extension value.
        /// </summary>
        object This { get; }

        /// <summary>
        /// Gets or sets collection item.
        /// </summary>
        object Item { get; }

        IImmutableDictionary<SoftString, IEqualityComparer<object>> Comparers { get; }

        IImmutableDictionary<SoftString, IExpression> References { get; }

        TreeNode<ExpressionDebugView> DebugView { get; }
    }

    [PublicAPI]
    public static class ExpressionScopeExtensions
    {
        public static IEnumerable<ExpressionScope> Enumerate(this ExpressionScope scope)
        {
            var current = scope;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }
    }
}