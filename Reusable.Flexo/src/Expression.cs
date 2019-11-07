using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.Utilities.JsonNet.Annotations;

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
        IExpression Next { get; }

        [NotNull]
        //IConstant Invoke();
        IConstant Invoke(IImmutableContainer context);
    }

    public interface IExtension
    {
        /// <summary>
        /// Indicates whether the extension is used as such or overrides it with its native value.
        /// </summary>
        bool IsInExtensionMode { get; }

        /// <summary>
        /// Gets the type the extension extends.
        /// </summary>
        Type ExtensionType { get; }
    }

    [Namespace("Flexo", Alias = "F")]
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
            typeof(Reusable.Flexo.Package),
            typeof(Reusable.Flexo.Import),
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

        protected Expression(ILogger? logger, SoftString name)
        {
            Logger = logger ?? EmptyLogger.Instance;
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [NotNull]
        protected ILogger Logger { get; }

        //[NotNull]
        //public static ExpressionScope Scope => ExpressionScope.Current ?? throw new InvalidOperationException("Expressions must be invoked within a valid scope. Use 'BeginScope' to introduce one.");

        public virtual SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public string Description { get; set; }

        public ISet<SoftString> Tags { get; set; }

        public bool Enabled { get; set; } = true;

        [JsonProperty("This")]
        public IExpression? Next { get; set; }

        //public abstract IConstant Invoke(IImmutableContainer context);
        
        public IConstant Invoke(IImmutableContainer context)
        {
            var parentView = context.Find(ExpressionContext.DebugView);
            var thisView = parentView.Add(this.CreateDebugView());

            // Take a shortcut when this is a constant without an extension. This helps to avoid another debug-view.
            if (this is IConstant constant && Next is null)
            {
                thisView.Value.Result = constant.Value;
                return ComputeConstant(context); // Constant.FromValue(constant.Name, constant.Value, context);// constant.Invoke(context);
            }

            var thisContext = context.BeginScope
            (
                ImmutableContainer
                    .Empty
                    .SetItem(ExpressionContext.DebugView, thisView)
            );

            var thisResult = ComputeConstant(thisContext);
            thisView.Value.Result = thisResult.Value;

            if (Next is IExtension extension && extension.IsInExtensionMode)
            {
                // Check whether result and extension match; do it only for extension expressions.
                var thisType =
                    thisResult.Value is IEnumerable<IExpression> collection
                        ? collection.GetType()
                        : thisResult.GetType();

                if (!extension.ExtensionType.IsAssignableFrom(thisType))
                {
                    throw DynamicException.Create
                    (
                        $"PipeTypeMismatch",
                        $"Extension '{extension.GetType().ToPrettyString()}<{extension.ExtensionType.ToPrettyString()}>' does not match the expression it is extending: '{thisResult.Value.GetType().ToPrettyString()}'."
                    );
                }
            }

            return Next?.Invoke
                   (
                       thisContext,
                       ImmutableContainer
                           .Empty
                           .SetItem(ExpressionContext.DebugView, thisView)
                           .SetItem(ExpressionContext.ThisOuter, thisResult)
                   ) ?? thisResult;
        }

        protected abstract IConstant ComputeConstant(IImmutableContainer context);

        protected class EmptyLogger : ILogger
        {
            public static EmptyLogger Instance { get; } = new EmptyLogger();

            public LoggerNode Node { get; }

            public T Use<T>(T next) where T : LoggerNode
            {
                return default;
            }

            public void Log(LogEntry logEntry) { }
        }
    }

    public static class ImmutableContainerExtensions
    {
        public static IImmutableContainer BeginScope(this IImmutableContainer context, IImmutableContainer scope)
        {
            return scope.SetItem(ExpressionContext.Parent, context);
        }

        public static IImmutableContainer BeginScopeWithThisOuter(this IImmutableContainer context, object thisOuter)
        {
            return context.BeginScope(ImmutableContainer.Empty.SetItem(ExpressionContext.ThisOuter, thisOuter));
        }

        /// <summary>
        /// Enumerates expression scopes from last to first. 
        /// </summary>
        public static IEnumerable<IImmutableContainer> Enumerate(this IImmutableContainer context)
        {
            do
            {
                yield return context;
                context = context.GetItemOrDefault(ExpressionContext.Parent);
            } while (context != null);
        }
    }

    public static class ExpressionExtensions
    {
        public static IConstant Invoke(this IExpression expression, IImmutableContainer context, IImmutableContainer scope)
        {
            return expression.Invoke(context.BeginScope(scope));
        }
    }
}