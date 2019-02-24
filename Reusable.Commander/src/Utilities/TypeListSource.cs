using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace Reusable.Commander.Utilities
{
    internal class TypeListSource<T> : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service is IServiceWithType swt && typeof(TypeList<T>).IsAssignableFrom(swt.ServiceType))
            {
                var registration =
                    new ComponentRegistration(
                        id: Guid.NewGuid(),
                        activator: new DelegateActivator(swt.ServiceType, (context, p) =>
                        {
                            var types =
                                context
                                    .ComponentRegistry
                                    .RegistrationsFor(new TypedService(typeof(T)))
                                    .Select(r => r.Activator)
                                    .OfType<ReflectionActivator>()
                                    .Select(activator => activator.LimitType);
                            return new TypeList<T>(types);
                        }),
                        services: new[] { service },
                        lifetime: new CurrentScopeLifetime(),
                        sharing: InstanceSharing.None,
                        ownership: InstanceOwnership.OwnedByLifetimeScope,
                        metadata: new Dictionary<string, object>()
                    );
                return new IComponentRegistration[] { registration };
            }
            // It's not a request for the base handler type, so skip it.
            else
            {
                return Enumerable.Empty<IComponentRegistration>();
            }
        }

        public bool IsAdapterForIndividualComponents => false;
    }

    public class TypeList<T> : List<Type>
    {
        internal TypeList(IEnumerable<Type> types) : base(types) { }
    }
}