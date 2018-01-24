using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig.Utilities
{
    public static class SettingSelector
    {
        public static (IConfiguration Configuration, TObject Object) From<TObject>([NotNull] this IConfiguration config)
        {
            return (config, default(TObject));
        }

        [CanBeNull]
        public static TValue Select<TObject, TValue>(this (IConfiguration Configuration, TObject obj) t, [NotNull] Expression<Func<TObject, TValue>> expression)
        {
            return t.Configuration.Select<TValue>(expression);
        }

        [CanBeNull]
        public static T Select<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            return config.Select<T>((LambdaExpression)expression, instance);
        }

        [CanBeNull]
        private static T Select<T>([NotNull] this IConfiguration config, [NotNull] LambdaExpression expression, [CanBeNull] string instance = null)
        {
            var name = expression.GetSettingName(instance);

            var settingDatastoreName = expression.GetCustomAttribute<SmartSettingAttribute>()?.DatastoreName;
            var setting = config.GetValue(name, typeof(T), settingDatastoreName) ?? GetDefaultValue();
            var validations = expression.GetCustomAttributes<ValidationAttribute>();
            Validate(validations, name, setting);

            return (T)setting;

            T GetDefaultValue()
            {
                var defaultValue = expression.GetCustomAttribute<DefaultValueAttribute>()?.Value;
                return defaultValue == null ? default(T) : (T)defaultValue;
            }
        }

        private static void Validate(IEnumerable<ValidationAttribute> validations, SettingName name, object value)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, $"Setting {name.ToString().QuoteWith("'")} is not valid.");
            }
        }

        [CanBeNull]
        public static T Select<T>(this IConfiguration config, [NotNull] SoftString settingName)
        {
            return (T)(config.GetValue(settingName, typeof(T), null) ?? default(T));
        }

        [NotNull]
        public static Lazy<T> LazySelect<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return new Lazy<T>(() => config.Select(expression, instance));
        }
    }

    public static class SettingUpdater
    {
        [NotNull]
        public static IConfiguration Update<TValue>([NotNull] this IConfiguration config, [NotNull] Expression<Func<TValue>> lambdaExpression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (lambdaExpression == null) throw new ArgumentNullException(nameof(lambdaExpression));

            var name = lambdaExpression.GetSettingName(instance);
            var value = lambdaExpression.GetValue();
            config.SetValue(name, value);
            return config;
        }
    }

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
                var value = configuration.GetValue(settingName, property.PropertyType, null);
                lambdaExpression.Set(value);
            }
            return configuration;
        }
    }
}