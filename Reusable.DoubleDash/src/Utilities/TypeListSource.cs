using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace Reusable.Commander.Utilities
{
    internal class TypeListSource : IRegistrationSource
    {
        // public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        // {
        //     throw new NotImplementedException();
        // }

        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (service is IServiceWithType swt && IsTypeList(swt.ServiceType))
            {
                var baseType = swt.ServiceType.GetGenericArguments().Single();

                var registration = new ComponentRegistration
                (
                    id: Guid.NewGuid(),
                    activator: new DelegateActivator(swt.ServiceType, (context, p) =>
                    {
//                        var types =
//                            context
//                                .ComponentRegistry
//                                .RegistrationsFor(new TypedService(baseType))
//                                .Select(r => r.Activator)
//                                .OfType<ReflectionActivator>()
//                                .Select(activator => activator.LimitType);
                        var types =
                            from r in context.ComponentRegistry.Registrations
                            from s in r.Services
                            let ts = s as TypedService
                            where ts is {} && baseType.IsAssignableFrom(ts.ServiceType)
                            select ts.ServiceType;
                        
                        var typeListCtor = typeof(TypeList<>).MakeGenericType(baseType).GetConstructor(new[] { typeof(IEnumerable<Type>) });
                        return typeListCtor.Invoke(new object[] { types });
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
            return Enumerable.Empty<IComponentRegistration>();
        }

        private static bool IsTypeList(Type serviceType)
        {
            return serviceType.IsGenericType && typeof(TypeList<>).IsAssignableFrom(serviceType.GetGenericTypeDefinition());
        }
    }

    public class TypeList<T> : List<Type>
    {
        public TypeList(IEnumerable<Type> types) : base(types) { }

        public Type ItemBaseType => typeof(T);
    }
}