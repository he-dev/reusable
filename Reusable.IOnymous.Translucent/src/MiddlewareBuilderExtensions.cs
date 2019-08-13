using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.IOnymous.Config;
using Reusable.IOnymous.Middleware;

namespace Reusable.IOnymous
{
    public static class MiddlewareBuilderExtensions
    {
        public static MiddlewareBuilder Use<T>(this MiddlewareBuilder builder, params object[] parameters)
        {
            return builder.Add<T>(parameters);
        }

        public static MiddlewareBuilder Use(this MiddlewareBuilder builder, Func<ResourceContext, RequestCallback<ResourceContext>, Task> lambda)
        {
            return builder.Add<LambdaMiddleware>(lambda);
        }

        public static MiddlewareBuilder UseResources(this MiddlewareBuilder builder, IEnumerable<IResourceProvider> providers)
        {
            return builder.Add<ResourceMiddleware>(providers);
        }
        
        public static MiddlewareBuilder UseResources(this MiddlewareBuilder builder, params IResourceProvider[] providers)
        {
            return builder.UseResources(providers.AsEnumerable());
        }

        public static MiddlewareBuilder UseTelemetry(this MiddlewareBuilder builder)
        {
            return builder.Add<TelemetryMiddleware>();
        }
        
//        public static MiddlewareBuilder UseAppSetting(this MiddlewareBuilder builder)
//        {
//            return builder.Add<AppSettingProvider>();
//        }
        
//        public static MiddlewareBuilder UseResourceProvider(this MiddlewareBuilder builder, IResourceProvider resourceProvider)
//        {
//            return builder.Add<AppSettingProvider>();
//        }
    }
}