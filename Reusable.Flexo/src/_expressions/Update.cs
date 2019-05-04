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
            var comparers = Scope.Find(Namespace, x => x.Comparers);
            var comparer = Comparer.IsNotNullOrEmpty() && comparers.TryGetValue(Comparer, out var c) ? c : comparers["Default"];


            var (obj, property, value) = Scope.Context.FindItem(First);
            var current = InvokeMethod(obj, property, value, comparer);
            var enumerableType = property.PropertyType.GetGenericArguments().First();
            var toList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(enumerableType);
            var cast = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(enumerableType);
            var casted = cast.Invoke(null, new object[] { current });
            var list = toList.Invoke(null, new object[] { casted });

            property.SetValue(obj, list);

            return (Name, ((IEnumerable<object>)list).Select((x, i) => Constant.Create($"Item-[{i}]", x)).ToList());
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
}