using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.Data
{
    public static class DtoUpdater
    {
        public static DtoUpdater<T> For<T>() => new DtoUpdater<T>(default);

        public static DtoUpdater<T> Update<T>(this T obj) => new DtoUpdater<T>(obj);
    }

    public class DtoUpdater<T>
    {
        private readonly T _obj;

        private readonly ICollection<(MemberInfo Member, object Value)> _updates = new List<(MemberInfo Member, object Value)>();

        public DtoUpdater(T obj) => _obj = obj;

        public DtoUpdater<T> With<TProperty>(Expression<Func<T, TProperty>> update, TProperty value)
        {
            _updates.Add((((MemberExpression)update.Body).Member, value));
            return this;
        }

        public T Commit()
        {
            var members =
                from member in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(m => m is PropertyInfo || m is FieldInfo)
                select (member.Name, Type: (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType);

            members = members.ToList();

            // Find the ctor that matches most properties.
            var ctors =
                from ctor in typeof(T).GetConstructors()
                let parameters = ctor.GetParameters()
                from parameter in parameters
                join member in members
                    on
                    new
                    {
                        Name = parameter.Name.AsIgnoreCase(),
                        Type = parameter.ParameterType
                    }
                    equals
                    new
                    {
                        Name = member.Name.AsIgnoreCase(),
                        Type = member.Type
                    }
                orderby parameters.Length descending
                select ctor;

            var theOne = ctors.First();

            // Join parameters and values by parameter order.
            // The ctor requires them sorted but they might be initialized in any order.
            var parameterValues =
                from parameter in theOne.GetParameters()
                join update in _updates on parameter.Name.AsIgnoreCase() equals update.Member.Name.AsIgnoreCase() into x
                from update in x.DefaultIfEmpty()
                select update.Value ?? GetMemberValueOrDefault(parameter.Name);

            return (T)theOne.Invoke(parameterValues.ToArray());
        }

        private object GetMemberValueOrDefault(string memberName)
        {
            if (_obj == null) return default;
            
            // There is for sure only one member with that name.
            switch (typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance).Single(m => m.Name.AsIgnoreCase().Equals(memberName)))
            {
                case PropertyInfo p: return p.GetValue(_obj);
                case FieldInfo f: return f.GetValue(_obj);
                default: return default; // Makes the compiler very happy.
            }
        }
    }

    internal static class StringExtensions
    {
        public static IEquatable<string> AsIgnoreCase(this string str) => (IgnoreCase)str;

        private class IgnoreCase : IEquatable<string>
        {
            private IgnoreCase(string value) => Value = value;
            private string Value { get; }
            public bool Equals(string other) => StringComparer.OrdinalIgnoreCase.Equals(Value, other);
            public override bool Equals(object obj) => obj is IgnoreCase ic && Equals(ic.Value);
            public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
            public static explicit operator IgnoreCase(string value) => new IgnoreCase(value);
        }
    }
}