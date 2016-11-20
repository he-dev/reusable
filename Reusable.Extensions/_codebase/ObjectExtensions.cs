using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Extensions
{
    public static class ObjectExtensions
    {
        public static Type GetElemenType(this object obj)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            if (obj.GetType().IsArray)
            {
                return obj.GetType().GetElementType(); ;
            }

            if (obj.GetType().IsList())
            {
                return obj.GetType().GetGenericArguments()[0];
            }

            if (obj.GetType().IsHashSet())
            {
                return obj.GetType().GetGenericArguments()[0];
            }

            if (obj.GetType().IsDictionary())
            {
                return obj.GetType().GetGenericArguments()[1];
            }

            return null;
        }
    }
}
