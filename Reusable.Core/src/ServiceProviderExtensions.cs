using System;

namespace Reusable {
    public static class ServiceProviderExtensions
    {
        public static object Resolve(this IServiceProvider serviceProvider, Type type) => serviceProvider.GetService(type); 
        
        public static T Resolve<T>(this IServiceProvider serviceProvider) => (T)serviceProvider.Resolve(typeof(T));
    }
}