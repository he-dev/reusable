using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class FunctionalExtensions
    {
        public static T Then<T>(this T obj, [NotNull] Action<T> mutate)
        {
            if (mutate == null) { throw new ArgumentNullException(nameof(mutate)); }

            mutate(obj);
            return obj;
        }
    }
}
