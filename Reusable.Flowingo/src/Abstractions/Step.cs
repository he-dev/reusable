using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Collections.Generic;
using Reusable.Flowingo.Annotations;

namespace Reusable.Flowingo.Abstractions
{
    public interface IStep<T> : INode<IStep<T>>
    {
        bool Enabled { get; set; }

        object Tag { get; }

        Task ExecuteAsync(T context);
    }

    public abstract class Step<T> : IStep<T>
    {
        protected Step(IServiceProvider? serviceProvider = default)
        {
            if (serviceProvider is {})
            {
                var serviceProperties =
                    from p in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where p.IsDefined(typeof(ServiceAttribute))
                    select p;

                foreach (var serviceProperty in serviceProperties)
                {
                    serviceProperty.SetValue(this, serviceProvider.GetService(serviceProperty.PropertyType));
                }
            }
        }

        public IStep<T>? Prev { get; set; }

        public IStep<T>? Next { get; set; }

        public bool Enabled { get; set; } = true;

        public object Tag { get; set; }

        public abstract Task ExecuteAsync(T context);

        protected async Task ExecuteNextAsync(T context)
        {
            foreach (var next in this.EnumerateNextWithoutSelf<IStep<T>>())
            {
                if (next.Enabled)
                {
                    try
                    {
                        await next.ExecuteAsync(context);
                    }
                    catch (Exception inner)
                    {
                        // todo - log
                    }
                }
                else
                {
                    // todo - log
                }
            }
        }

        public static IStep<T> Empty(object tag) => new Relay { Tag = tag };

        private class Relay : Step<T>
        {
            public override Task ExecuteAsync(T context) => ExecuteNextAsync(context);
        }
    }
}