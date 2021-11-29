using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Utilities.JsonNet.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    using static ExpressionContext;

    public interface ISwitchable
    {
        [DefaultValue(true)]
        bool Enabled { get; }
    }

    public interface IIdentifiable
    {
        SoftString Id { get; }
    }

    [UsedImplicitly]
    [PublicAPI]
    public interface IExpression : IIdentifiable, ISwitchable
    {
        string? Description { get; }

        ISet<SoftString>? Tags { get; }

        IExpression? Next { get; }

        IConstant Invoke(IImmutableContainer context);
    }

    public interface IExtension : IExpression
    {
        /// <summary>
        /// Indicates whether the extension is used as such or overrides it with its native value.
        /// </summary>
        bool ArgMustMatch { get; }

        /// <summary>
        /// Gets the type the extension extends.
        /// </summary>
        Type ExtendsType { get; }
    }

    [PublicAPI]
    [JsonTypeSchema("Flexo", Alias = "F")]
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
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
            typeof(Reusable.Flexo.DateTime),
            typeof(Reusable.Flexo.TimeSpan),
            typeof(Reusable.Flexo.Collection),
            typeof(Reusable.Flexo.Select),
            typeof(Reusable.Flexo.Throw),
            typeof(Reusable.Flexo.Where),
            typeof(Reusable.Flexo.Block),
            typeof(Reusable.Flexo.ForEach),
            //typeof(Reusable.Flexo.Item),
        };
        // ReSharper restore RedundantNameQualifier

        //private static volatile int _counter = 0;

        private SoftString _name;

        protected Expression(ILogger? logger)
        {
            //Logger = logger ?? EmptyLogger.Instance;
            //_name = $"{GetType().ToPrettyString()}-{++_counter}"!;
            _name = GetType().ToPrettyString();
        }

        public SoftString Id
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Id));
        }

        public string? Description { get; set; }

        public ISet<SoftString>? Tags { get; set; } = new HashSet<SoftString>();

        public bool Enabled { get; set; } = true;

        [JsonProperty("This")]
        public IExpression? Next { get; set; }

        public static IConstant InvokePackage(string packageId, IImmutableContainer context)
        {
            return context.FindItem(Packages, packageId).Invoke(context);
        }

        public IConstant Invoke(IImmutableContainer context)
        {
            // This prevents exceptions in case of a forgotten main-scope.
            var startsWithScopeMain = context.Scopes().Last().GetItemOrDefault(ExpressionContext.Id, string.Empty).Equals(Keywords.Main);
            context = startsWithScopeMain ? context : Default.BeginScope(context);

            var parentLog = context.FindItem(InvokeLog);
            var thisLog = this is IConstant ? parentLog : parentLog.Add(CreateInvokeLog());
            var thisContext = context.SetItem(InvokeLog, thisLog);
            var thisResult = ComputeConstant(thisContext);

            thisLog.Add(thisResult);

            if (Next is IExtension extension && extension.ArgMustMatch)
            {
                ValidateArgMatches(thisResult, extension);
            }

            return Next?.Invoke(thisContext.BeginScopeWithArg(thisResult)) ?? thisResult;
        }

        // Check whether result and extension match; do it only for extension expressions.
        private static void ValidateArgMatches(IConstant arg, IExtension extension)
        {
            if (!extension.ExtendsType.IsAssignableFrom(arg.ValueType))
            {
                throw DynamicException.Create
                (
                    $"ExtensionTypeMismatch",
                    $"Extension '{extension.GetType().ToPrettyString()}<{extension.ExtendsType.ToPrettyString()}>' does not match the expression it is extending: '{arg.ValueType.ToPrettyString()}'."
                );
            }
        }

        private Node<IExpression> CreateInvokeLog() => Node.Create<IExpression>(this);

        protected abstract IConstant ComputeConstant(IImmutableContainer context);
    }
}