using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Extensions
{
    public static class HashSetExtensions
    {
        public static bool Add<T>(this ISet<T> set, IEnumerable<T> source)
        {
            var addedCount = 0;
            var itemCount = 0;
            foreach (var item in source)
            {
                 addedCount += set.Add(item) ? 1 :0;
                itemCount++;
            }
            return addedCount == itemCount;
        }
    }
}
