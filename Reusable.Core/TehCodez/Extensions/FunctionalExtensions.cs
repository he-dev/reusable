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
        [CanBeNull, ContractAnnotation("obj: null => null; obj: notnull => notnull")]
        public static T Next<T>([CanBeNull] this T obj, [NotNull] Action<T> next)
        {
            if (next == null) { throw new ArgumentNullException(nameof(next)); }

            next(obj);
            return obj;
        }
    }
}
