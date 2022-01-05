using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.ReMember;

[PublicAPI]
public abstract class SelectorBuilder<T>
{
    public static IEnumerable<Selector> Selectors
    {
        get
        {
            //var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance).Concat(typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Static));
            var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Static);
            return
                from m in members
                where m is PropertyInfo or FieldInfo
                select new Selector(typeof(T), m);
        }
    }

    public static Selector<TMember> Select<TMember>(Expression<Func<Selector<TMember>>> selector)
    {
        return new Selector<TMember>(selector);
    }
}