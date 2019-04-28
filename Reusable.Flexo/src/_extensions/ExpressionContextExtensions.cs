using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public static class ExpressionContextExtensions
    {
        #region Extension-Input helpers

        public static IImmutableSession PushExtensionInput(this IImmutableSession context, object value)
        {
            context.Get(Use<IExpressionSession>.Scope, x => x.ExtensionInputs).Push(value);
            return context;
        }

        // The Input must be removed so that subsequent expression doesn't 'think' it's called as extension when it isn't.
        public static bool TryPopExtensionInput<T>(this IImmutableSession context, out T input)
        {
            var inputs = context.Get(Use<IExpressionSession>.Scope, x => x.ExtensionInputs);
            if (inputs.Any())
            {
                input = (T)inputs.Pop();
                return true;
            }
            else
            {
                input = default;
                return false;
            }
        }

        #endregion

        public static IImmutableSession WithComparer(this IImmutableSession context, string name, IEqualityComparer<object> comparer)
        {
            var scope = Use<IExpressionSession>.Scope;
            var comparers = 
                context
                    .Get(scope, x => x.Comparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                    .SetItem(name, comparer);
            
            return context.Set(scope, x => x.Comparers, comparers);            
        }

        public static IImmutableSession WithDefaultComparer(this IImmutableSession context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparer<object>.Default);
        }
        
        public static IImmutableSession WithSoftStringComparer(this IImmutableSession context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            ));
        }

        public static IImmutableSession WithRegexComparer(this IImmutableSession context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => Regex.IsMatch((string)right, (string)left, RegexOptions.None),
                getHashCode: (obj) => 0
            ));
        }

        public static string GetComparerNameFromCaller([CallerMemberName] string comparerName = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Regex.Match(comparerName, "^With(?<comparerName>[a-z0-9_]+)Comparer", RegexOptions.IgnoreCase).Groups["comparerName"].Value;
        }
        
        public static IImmutableSession WithExpressions(this IImmutableSession context, IEnumerable<IExpression> expressions)
        {
            var scope = Use<IExpressionSession>.Scope;
            var registrations = 
                context
                    .Get(scope, x => x.Expressions, ImmutableDictionary<SoftString, IExpression>.Empty)
                    .SetItems(expressions.Select(e => new KeyValuePair<SoftString, IExpression>($"R.{e.Name.ToString()}", e)));
            return context.Set(scope, x => x.Expressions, registrations);
        }

        public static IEqualityComparer<object> GetComparerOrDefault(this IImmutableSession context, string name)
        {
            var comparers = context.Get(Use<IExpressionSession>.Scope, x => x.Comparers);
            if (name is null)
            {
                return comparers["Default"];
            }

            if (comparers.TryGetValue(name, out var comparer))
            {
                return comparer;
            }

            throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.");
        }

        public static TreeNode<ExpressionDebugView> DebugView(this IImmutableSession context)
        {
            return context.Get(Use<IExpressionSession>.Scope, x => x.DebugView);
        }

        public static IImmutableSession DebugView(this IImmutableSession context, TreeNode<ExpressionDebugView> debugView)
        {
            return context.Set(Use<IExpressionSession>.Scope, x => x.DebugView, debugView);
        }
    }
}