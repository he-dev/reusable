using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    public interface ISwitchable
    {
        [DefaultValue(true)]
        bool Enabled { get; }
    }

    public interface IExtendable
    {
        List<IExpression> This { get; }
    }

    [UsedImplicitly]
    [PublicAPI]
    public interface IExpression : ISwitchable, IExtendable
    {
        [NotNull]
        SoftString Name { get; }

        [NotNull]
        IExpressionContext Context { get; }

        [NotNull]
        IExpression Invoke([NotNull] IExpressionContext context);
    }

    public interface IExtension<T>
    {
        //IExpression This { get; }
    }

    [Namespace("Flexo")]
    public abstract class Expression : IExpression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            typeof(Reusable.Flexo.ObjectEqual),
            typeof(Reusable.Flexo.StringEqual),
            typeof(Reusable.Flexo.GreaterThan),
            typeof(Reusable.Flexo.GreaterThanOrEqual),
            typeof(Reusable.Flexo.LessThan),
            typeof(Reusable.Flexo.LessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.ToDouble),
            typeof(Reusable.Flexo.ToString),            
            typeof(Reusable.Flexo.GetContextItem),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.Overlaps),
            typeof(Reusable.Flexo.Matches),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Count),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.ToList),
            typeof(Reusable.Flexo.Constant<>),
            typeof(Reusable.Flexo.Double),
            typeof(Reusable.Flexo.Integer),
            typeof(Reusable.Flexo.Decimal),
            typeof(Reusable.Flexo.DateTime),
            typeof(Reusable.Flexo.TimeSpan),
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
            typeof(Reusable.Flexo.Collection),
            typeof(Reusable.Flexo.SoftStringComparer),
            typeof(Reusable.Flexo.IsEqual),
        };
        // ReSharper restore RedundantNameQualifier

        protected Expression([NotNull] SoftString name, [NotNull] IExpressionContext context)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual SoftString Name { get; }

        public IExpressionContext Context { get; }

        public bool Enabled { get; set; } = true;

        public List<IExpression> This { get; set; } = new List<IExpression>();

        public virtual IExpression Invoke(IExpressionContext context)
        {
            var extensions = This ?? Enumerable.Empty<IExpression>();
            return
                extensions
                    .Enabled()
                    .Aggregate
                    (
                        InvokeCore(context),
                        (previous, next) => next.Invoke(previous.Context.Set(Item.For<IExtensionContext>(), x => x.Input, previous))
                    );
        }

        protected abstract IExpression InvokeCore(IExpressionContext context);

        /// <summary>
        /// Determines whether the expression is invoked as an extension and removes the Input from the context.
        /// </summary>
        private bool IsExtension(ref IExpressionContext context, out IExpression input)
        {
            var inputKey = ExpressionContext.CreateKey(Item.For<IExtensionContext>(), x => x.Input);
            if (context.TryGetValue(inputKey, out var value))
            {
                input = (IExpression)value;
                // The Input must be removed so that subsequent expression don't 'think' they are called as extensions when they aren't.
                context = context.Remove(inputKey);
                return true;
            }
            else
            {
                input = default;
                return false;
            }
        }

        protected IExpression ExtensionInputOrDefault(ref IExpressionContext context, object value)
        {
            return ExtensionInputOrDefault(ref context, new[] { value }).Single();                
        }
        
        protected IEnumerable<IExpression> ExtensionInputOrDefault(ref IExpressionContext context, IEnumerable<object> values)
        {
            return
                IsExtension(ref context, out var input)
                    ? input.Value<IEnumerable<IExpression>>()
                    : values.Select(Constant.FromValueOrDefault("Value"));
        }
    }
    
    [PublicAPI]
    public abstract class Expression<T> : Expression
    {
        protected Expression(string name, IExpressionContext context) : base(name, context) { }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            return Constant.FromResult(Name, Calculate(context));
        }

        protected abstract CalculateResult<T> Calculate(IExpressionContext context);
    }

    public interface IExtensionContext
    {
        IExpression Input { get; }
    }

    public readonly struct CalculateResult<T>
    {
        private CalculateResult(T value, IExpressionContext context)
        {
            Value = value;
            Context = context;
        }

        public T Value { get; }

        public IExpressionContext Context { get; }

        public void Deconstruct(out T value, out IExpressionContext context)
        {
            value = Value;
            context = Context;
        }

        public static implicit operator CalculateResult<T>((T Value, IExpressionContext Context) t) => new CalculateResult<T>(t.Value, t.Context);
    }    
}