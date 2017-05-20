using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable
{
    public static class Reflection
    {
        public static bool IsStatic<T>() => typeof(T).IsStatic();

        public static bool Implements<T, TCheck>() => typeof(T).Implements(typeof(TCheck));


        // --- non-extensions        

        [CanBeNull]
        public static Type GetElementType([NotNull] Type type)
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

        [CanBeNull]
        public static Type GetElementType([NotNull] object obj)
        {
            return GetElementType(obj?.GetType());
        }

        [CanBeNull]
        public static Type GetElementType<T>()
        {
            return GetElementType(typeof(T));
        }
    }
}
