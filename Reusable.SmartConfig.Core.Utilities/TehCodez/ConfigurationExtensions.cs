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
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.SmartConfig.Utilities
{
    public static class ConfigurationExtensions
    {
        //public static (IConfiguration Configuration, TObject Object) From<TObject>([NotNull] this IConfiguration config)
        //{
        //    return (config, default(TObject));
        //}

        //[CanBeNull]
        //public static TValue GetValue<TObject, TValue>(this (IConfiguration Configuration, TObject obj) t, [NotNull] Expression<Func<TObject, TValue>> expression)
        //{
        //    return t.Configuration.GetValue<TValue>(expression);
        //}

        #region GetValue overloads

        [CanBeNull]
        private static T GetValue<T>([NotNull] this IConfiguration config, [NotNull] LambdaExpression expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingName = expression.GetSettingName(instance);
            var settingDataStoreName = expression.GetCustomAttribute<SmartSettingAttribute>()?.DataStoreName;
            var settingValue = config.GetValue(settingName, typeof(T), settingDataStoreName) ?? expression.GetCustomAttribute<DefaultValueAttribute>()?.Value;

            expression
                .GetCustomAttributes<ValidationAttribute>()
                .Validate(settingName, settingValue);

            return (T)settingValue;
        }

        [CanBeNull]
        public static T GetValue<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return config.GetValue<T>((LambdaExpression)expression, instance);
        }

        [CanBeNull]
        public static T GetValue<T>(this IConfiguration config, [NotNull] SoftString settingName)
        {
            return (T)config.GetValue(settingName, typeof(T), null);
        }

        [NotNull]
        public static Lazy<T> GetValueLazy<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return Lazy.Create(() => config.GetValue(expression, instance));
        }

        #endregion

        #region SetValue overloads

        [NotNull]
        public static IConfiguration SetValue<TValue>([NotNull] this IConfiguration config, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingName = expression.GetSettingName(instance);
            var settingValue = expression.GetValue();

            expression
                .GetCustomAttributes<ValidationAttribute>()
                .Validate(settingName, settingValue);

            config.SetValue(settingName, settingValue, null);
            return config;
        }

        #endregion

        #region AssignValue overloads

        [NotNull]
        public static IConfiguration AssignValue<TValue>([NotNull] this IConfiguration configuration, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var value = configuration.GetValue(expression, instance);
            expression.Set(value);
            return configuration;
        }

        [NotNull]
        public static IConfiguration AssignValues<T>([NotNull] this IConfiguration configuration, [NotNull] T obj, [CanBeNull] string instance = null)
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

        #endregion

        private static void Validate(this IEnumerable<ValidationAttribute> validations, SettingName settingName, object value)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, $"Setting {settingName.ToString().QuoteWith("'")} is not valid.");
            }
        }
    }
}