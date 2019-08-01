using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public abstract class Update : Expression<IEnumerable<IExpression>>
    {
        protected Update([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        public string First { get; set; }

        public IEnumerable<IExpression> Second { get; set; }

        public string Comparer { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore()
        {
            var comparer = Scope.GetComparerOrDefault(Comparer);
            var (obj, property, value) = Scope.Context.FindItem(First);
            var current = InvokeMethod(obj, property, value, comparer);
            var enumerableType = property.PropertyType.GetGenericArguments().First();
            var list = current.ToList(enumerableType);

            property.SetValue(obj, list);

            return (Name, ((IEnumerable<object>)list).Select((x, i) => Constant.FromValue($"Item-[{i}]", x)).ToList());
        }

        protected abstract IEnumerable<object> InvokeMethod(object obj, PropertyInfo property, object value, IEqualityComparer<object> comparer);

       
    }

    public class Concat : Update
    {
        public Concat([NotNull] ILogger<Concat> logger) : base(logger, nameof(Concat)) { }

        protected override IEnumerable<object> InvokeMethod(object obj, PropertyInfo property, object value, IEqualityComparer<object> comparer)
        {
            return ((IEnumerable<object>)value).Concat(Second.Invoke().Values<object>());
        }
    }

    public class Union : Update
    {
        public Union([NotNull] ILogger<Union> logger) : base(logger, nameof(Union)) { }

        protected override IEnumerable<object> InvokeMethod(object obj, PropertyInfo property, object value, IEqualityComparer<object> comparer)
        {
            return ((IEnumerable<object>)value).Union(Second.Invoke().Values<object>(), comparer);
        }
    }

    public class SetSingle : Expression<object>
    {
        public SetSingle([NotNull] ILogger<SetSingle> logger) : base(logger, nameof(SetSingle)) { }

        public string Path { get; set; }

        public IExpression Value { get; set; }

        protected override Constant<object> InvokeCore()
        {
            var (obj, property, value) = Scope.Context.FindItem(Path);
            value = Value.Invoke().Value;
            property.SetValue(obj, value);
            return (Name, default);
        }
    }

    public class SetMany : Expression<object>
    {
        public SetMany([NotNull] ILogger<SetMany> logger) : base(logger, nameof(SetMany)) { }

        public string Path { get; set; }

        public IEnumerable<IExpression> Values { get; set; }

        protected override Constant<object> InvokeCore()
        {
            var (obj, property, value) = Scope.Context.FindItem(Path);
            var enumerableType = property.PropertyType.GetGenericArguments().First();
            var list = Values.Invoke().Values<object>().ToList(enumerableType);

            property.SetValue(obj, list);
            return (Name, default);
        }
    }    
}