using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Reusable.Tests.XUnit.Experimental {
    public class DuckObject<T> : DynamicObject
    {
        private static readonly DuckObject<T> Duck = new DuckObject<T>();

        public static TValue Quack<TValue>(Func<dynamic, dynamic> quack)
        {
            return (TValue)quack(Duck);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            throw new InvalidOperationException($"Cannot use an indexer on '{typeof(T)}' because static types do not have them.");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var member = typeof(T).GetMember(binder.Name).SingleOrDefault();
            switch (member?.MemberType)
            {
                case MemberTypes.Field:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.GetField, null, null, null);
                    break;
                case MemberTypes.Property:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.GetProperty, null, null, null);
                    break;
                default:
                    throw new StaticMemberNotFoundException<T>(binder.Name);
            }

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var member = typeof(T).GetMember(binder.Name).SingleOrDefault();
            switch (member?.MemberType)
            {
                case MemberTypes.Method:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, null, args);
                    break;
                default:
                    throw new StaticMemberNotFoundException<T>(binder.Name);
            }

            return true;
        }
    }
    
    public class DuckObject
    {
        private static readonly ConcurrentDictionary<Type, dynamic> Cache = new ConcurrentDictionary<Type, dynamic>();

        public static TValue Quack<TValue>(Type type, Func<dynamic, dynamic> quack)
        {
            var duck = Cache.GetOrAdd(type, t => Activator.CreateInstance(typeof(DuckObject<>).MakeGenericType(type)));
            return (TValue)quack(duck);
        }
    }
}