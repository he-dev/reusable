using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Reusable.Extensions;

namespace Reusable
{
    public static class Reflection
    {
        public static bool IsStatic<T>() => typeof(T).IsStatic();

        public static bool Implements<T, TCheck>() => typeof(T).Implements(typeof(TCheck));


        // --- non-extensions        

        public static Type GetElemenType(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (type.IsArray)
            {
                return type.GetElementType(); ;
            }

            if (type.IsList())
            {
                return type.GetGenericArguments()[0];
            }

            if (type.IsHashSet())
            {
                return type.GetGenericArguments()[0];
            }

            if (type.IsDictionary())
            {
                return type.GetGenericArguments()[1];
            }

            return null;
        }

        public static Type GetElemenType(object obj)
        {
            return GetElemenType(obj?.GetType());
        }

        public static Type GetElemenType<T>()
        {
            return GetElemenType(typeof(T));
        }
    }
}
