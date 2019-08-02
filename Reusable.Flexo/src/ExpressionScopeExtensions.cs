using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
    [PublicAPI]
    public static class ExpressionScopeExtensions
    {
        /// <summary>
        /// Enumerates expression scopes from last to first. 
        /// </summary>
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