using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig.Utilities
{
    public static class ConfigurationExtensions
    {
        [NotNull]
        public static IConfiguration Assign<TValue>([NotNull] this IConfiguration configuration, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var value = configuration.Select(expression, instance);
            expression.Set(value);
            return configuration;
        }

        [NotNull]
        public static IConfiguration Assign<T>([NotNull] this IConfiguration configuration, [NotNull] T obj, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttribute<SmartSettingAttribute>() != null);
            foreach (var property in properties)
            {
                // Create a lambda-expression so that we can reuse the extensions for it we already have.
                var lambdaExpression = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(obj), 
                        property.Name
                    )
                );
                var settingName = lambdaExpression.GetSettingName(instance);
                var value = configuration.Select(settingName, property.PropertyType, null);
                lambdaExpression.Set(value);
            }
            return configuration;
        }
    }
}