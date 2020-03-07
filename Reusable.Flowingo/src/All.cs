using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Flowingo
{
    public class All<T> : List<IPredicate<T>>, IPredicate<T>
    {
        public void Add(Func<T, bool> predicate) => Add(new Predicate<T>(predicate));

        public bool Invoke(T context) => this.All(p => p.Invoke(context));
    }

    public static class WorkflowHelpers
    {
        public static IPredicate<T> Not<T>(this IPredicate<T> builder) => new Not<T>(builder);
    }

    public class Not<T> : IPredicate<T>
    {
        public Not(IPredicate<T> predicate) => Predicate = predicate;

        private IPredicate<T> Predicate { get; }

        public bool Invoke(T context) => !Predicate.Invoke(context);
    }

    public interface IPredicate<in T>
    {
        bool Invoke(T context);
    }

    public class Predicate<T> : IPredicate<T>
    {
        private readonly Func<T, bool> _predicate;

        public Predicate(Func<T, bool> predicate) => _predicate = predicate;
        
        public static IPredicate<T> True => new Predicate<T>(_ => true);
        
        public static IPredicate<T> False => new Predicate<T>(_ => false);

        public virtual bool Invoke(T context) => _predicate.Invoke(context);
    }
}