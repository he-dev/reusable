using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Services;

namespace Reusable.SmartConfig
{
    public static class ConfigurationExtensions
    {
        private static readonly IDictionary<CaseInsensitiveString, object> Cache = new Dictionary<CaseInsensitiveString, object>();

        private const string NamespaceSeparator = "+";

        private const string InstanceSeparator = ",";

        public static IConfiguration Apply<T>(this IConfiguration configuration, Expression<Func<T>> expression, string instance = null)
        {
            var value = configuration.Select(expression, instance);
            expression.Apply(value);
            return configuration;
        }

        public static T Select<T>(this IConfiguration config, Expression<Func<T>> expression, string instance = null, bool cached = false)
        {
            var smartConfig = expression.GetSmartSettingAttribute();
            var name = smartConfig?.Name ?? expression.CreateName(instance);

            if (cached && Cache.TryGetValue(name, out var value)) { return (T)value; }

            var setting = config.Select<T>(name, smartConfig?.Datasource, expression.GetDefaultValue());
            expression.Validate(setting, name);

            if (cached) { Cache[name] = setting; }

            return setting;
        }

        public static IConfiguration Update<T>(this IConfiguration config, Expression<Func<T>> expression, string instance = null)
        {
            var smartConfig = expression.GetSmartSettingAttribute();
            var name = smartConfig?.Name ?? expression.CreateName(instance);
            config.Update(name, expression.Select());
            return config;
        }

        private static CaseInsensitiveString CreateName<T>(this Expression<Func<T>> expression, string instance)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");

            // Namespace+Object.Property,Instance
            return
                $"{memberExpr.Member.DeclaringType.Namespace}" +
                $"{NamespaceSeparator}" +
                $"{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}" +
                (string.IsNullOrEmpty(instance) ? string.Empty : $"{InstanceSeparator}{instance}");
        }

        private static SmartSettingAttribute GetSmartSettingAttribute<T>(this Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            return memberExpr.Member.GetCustomAttribute<SmartSettingAttribute>();
        }

        private static object GetDefaultValue<T>(this Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            return memberExpr.Member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        }

        private static void Validate<T>(this Expression<Func<T>> expression, object value, CaseInsensitiveString name)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            foreach (var validation in memberExpr.Member.GetCustomAttributes<ValidationAttribute>())
            {
                validation.Validate(value, $"Setting '{name}' is not valid.");
            }
        }
    }
}