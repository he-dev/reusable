using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    public abstract class Extension<TArg, TResult> : Expression<TResult>, IExtension //where TArg : class
    {
        protected Extension(ILogger? logger) : base(logger) { }

        /// <summary>
        /// Gets or sets the local Arg that overrides the one passed via context.
        /// </summary>
        protected object? Arg { get; set; }

        #region IExtension

        bool IExtension.ArgMustMatch => Arg is null;

        Type IExtension.ExtendsType => typeof(TArg);

        #endregion

        protected IConstant GetArg(IImmutableContainer context)
        {
            var arg = Arg is {} a ? a : context.FindItem(ExpressionContext.Arg);

            return arg switch
            {
                IConstant c => c,
                IEnumerable<IExpression> s => new Constant<TArg>("Arg", s.Enabled().SelectMany(e => e.Invoke(context).Cast<TArg>()), context),
                IExpression e => new Constant<TArg>("Arg", e.Invoke(context).Cast<TArg>(), context),
                _ => throw DynamicException.Create("ArgNotFound", $"Could not find {nameof(Arg)} in any context.")
            };
        }
    }
}