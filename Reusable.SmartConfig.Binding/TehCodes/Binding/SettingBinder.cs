using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Helpers;

namespace Reusable.SmartConfig.Binding
{
    public static class SettingBinder
    {
        [NotNull]
        public static IConfiguration Bind<TValue>([NotNull] this IConfiguration configuration, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var value = configuration.Select(expression, instance);
            expression.Bind(value);
            return configuration;
        }

        [NotNull]
        public static IConfiguration Bind<T>([NotNull] this IConfiguration configuration, [NotNull] T obj, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttribute<SmartSettingAttribute>() != null);
            foreach (var property in properties)
            {
                // Create a lambda-expression so that we can reuse the extensions for it we already have.
                var lambdaExpr = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(obj), 
                        property.Name
                    )
                );
                var settingName = lambdaExpr.GetSettingName(instance);
                var value = configuration.Select(settingName, property.PropertyType, null);
                lambdaExpr.Bind(value);
            }
            return configuration;
        }
    }
}