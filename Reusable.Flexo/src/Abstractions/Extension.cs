using System;
using System.Collections;
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
                IConstant c => c, //new Constant<TArg>("Arg", arg.Cast<TArg>(), context),
                IEnumerable<IExpression> s => new Constant<TArg>("Arg", s.Enabled().SelectMany(e => e.Invoke(context).Cast<TArg>()), context),
                IExpression e => new Constant<TArg>("Arg", e.Invoke(context).Cast<TArg>(), context),
                _ => throw DynamicException.Create("ArgNotFound", $"Could not find {nameof(Arg)} in any context.")
            };
        }

//        private static IEnumerable<TArg> InvokeArgs(IEnumerable args, IImmutableContainer context)
//        {
//            foreach (var arg in args)
//            {
//                var result = arg switch
//                {
//                    IConstant c => c.Cast<TArg>(),
//                    //IExpression e => e.Invoke(context).Cast<TArg>(),
//                    _ => Enumerable.Empty<TArg>()
//                };
//
//                foreach (var item in result)
//                {
//                    yield return item;
//                }
//            }
//        }
    }

//    public abstract class ScalarExtension<TResult> : Extension<IExpression, TResult>
//    {
//        protected ScalarExtension(ILogger? logger) : base(logger) { }
//
//        protected override IExpression GetArg(IImmutableContainer context)
//        {
//            if (Arg is {})
//            {
//                return Arg;
//            }
//
//            return context.FindItem(ExpressionContext.Arg) is var item && item is IExpression expression ? expression : throw DynamicException.Create("ArgNotFound", $"Could not find {nameof(Arg)} in any context.");
//        }
//
//        protected IConstant InvokeArg(IImmutableContainer context, IImmutableContainer? scope = default)
//        {
//            return GetArg(context).Invoke(context, scope);
//        }
//    }
//
//    public abstract class CollectionExtension<TResult> : Extension<IEnumerable<IExpression>, TResult>
//    {
//        protected CollectionExtension(ILogger? logger) : base(logger) { }
//
//        /// <summary>
//        /// Gets enabled expressions.
//        /// </summary>
//        protected override IEnumerable<IExpression> GetArg(IImmutableContainer context)
//        {
//            if (Arg is {})
//            {
//                return Arg.Enabled();
//            }
//
//            var arg = context.FindItem(ExpressionContext.Arg) is var item && item is IConstant constant ? constant.Value : item;
//            return arg is IEnumerable<IExpression> expressions ? expressions.Enabled() : throw DynamicException.Create("ArgNotFound", $"Could not find {nameof(Arg)} in any context.");
//        }
//
//        protected IEnumerable<IConstant> InvokeArg(IImmutableContainer context, IImmutableContainer? scope = default)
//        {
//            return GetArg(context).Select(x => x.Invoke(context, scope));
//        }
//    }
}