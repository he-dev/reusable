using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Helpers;

namespace Reusable.SmartConfig
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
            var setting = config.Select(name, typeof(T), settingDatastoreName) ?? GetDefaultValue();
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
            return (T)(config.Select(settingName, typeof(T), null) ?? default(T));
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
            config.Update(name, value);
            return config;
        }
    }
}