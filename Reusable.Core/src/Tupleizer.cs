using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Reflection;

namespace Reusable
{
    internal static class Tupleizer
    {
        public const int SuccessKey = 0;

        public static Tuple<bool, Tuple<T1, T2>> Tupleize<T1, T2>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3>> Tupleize<T1, T2, T3>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4>> Tupleize<T1, T2, T3, T4>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5>> Tupleize<T1, T2, T3, T4, T5>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4),
                        data.GetValue<T5>(5)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5, T6>> Tupleize<T1, T2, T3, T4, T5, T6>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4),
                        data.GetValue<T5>(5),
                        data.GetValue<T6>(6)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5, T6, T7>> Tupleize<T1, T2, T3, T4, T5, T6, T7>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4),
                        data.GetValue<T5>(5),
                        data.GetValue<T6>(6),
                        data.GetValue<T7>(7)
                    )
                );
        }

        private static T GetValue<T>(this IDictionary<int, object> data, int key)
        {
            if (data.TryGetValue(key, out var value))
            {
                var valueType = typeof(T);
                var targetType =
                    valueType.IsNullable()
                        ? valueType.GetGenericArguments().Single()
                        : valueType;

                return (T)Convert.ChangeType(value, targetType);
            }
            else
            {
                return default;
            }
        }
        
        // -----------
        
        
    }
}