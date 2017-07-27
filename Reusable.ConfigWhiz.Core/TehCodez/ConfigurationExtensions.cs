using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Services;

namespace Reusable.SmartConfig
{
    public static class ConfigurationExtensions
    {
        private static readonly IDictionary<CaseInsensitiveString, object> Cache = new Dictionary<CaseInsensitiveString, object>();

        [NotNull]
        public static IConfiguration Apply<TValue>([NotNull] this IConfiguration configuration, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null)
        {
            var value = configuration.Select(expression, instance);
            expression.Apply(value);
            return configuration;
        }

        public static (IConfiguration Configuration, TObject Object) For<TObject>([NotNull] this IConfiguration config)
        {
            return (config, default(TObject));
        }

        [CanBeNull]
        public static TValue Select<TObject, TValue>(this (IConfiguration Configuration, TObject obj) t, [NotNull] Expression<Func<TObject, TValue>> expression)
        {
            return t.Configuration.Select<TValue>(expression);
        }

        [CanBeNull]
        public static TValue Select<TValue>([NotNull] this IConfiguration config, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null, bool cached = false)
        {
            return config.Select<TValue>((LambdaExpression)expression, instance, cached);
        }

        [CanBeNull]
        private static TValue Select<TValue>([NotNull] this IConfiguration config, [NotNull] LambdaExpression expression, [CanBeNull] string instance = null, bool cached = false)
        {
            var smartConfig = expression.GetSmartSettingAttribute();
            var name = smartConfig?.Name ?? expression.CreateName(instance);

            if (cached && Cache.TryGetValue(name, out var value)) { return (TValue)value; }

            var setting = config.Select<TValue>(name, smartConfig?.Datasource, expression.GetDefaultValue());
            expression.Validate(setting);

            if (cached) { Cache[name] = setting; }

            return setting;
        }

        [NotNull]
        public static IConfiguration Update<TValue>([NotNull] this IConfiguration config, [NotNull] Expression<Func<TValue>> expression, [CanBeNull] string instance = null)
        {
            var smartConfig = expression.GetSmartSettingAttribute();
            var name = smartConfig?.Name ?? expression.CreateName(instance);
            config.Update(name, expression.Select());
            return config;
        }
    }

    internal static class ExpressionExtensions
    {
        private const string NamespaceSeparator = "+";

        private const string InstanceSeparator = ",";

        public static CaseInsensitiveString CreateName([NotNull] this LambdaExpression lambdaExpression, [CanBeNull] string instance = null)
        {
            var memberExpr = lambdaExpression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");

            // Namespace+Object.Property,Instance
            return
                $"{memberExpr.Member.DeclaringType.Namespace}" +
                $"{NamespaceSeparator}" +
                $"{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}" +
                (String.IsNullOrEmpty(instance) ? String.Empty : $"{InstanceSeparator}{instance}");
        }

        public static SmartSettingAttribute GetSmartSettingAttribute([NotNull] this LambdaExpression lambdaExpression)
        {
            var memberExpr = lambdaExpression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            return memberExpr.Member.GetCustomAttribute<SmartSettingAttribute>();
        }

        public static object GetDefaultValue([NotNull] this LambdaExpression expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            return memberExpr.Member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        }

        public static void Validate([NotNull] this LambdaExpression lambdaExpression, [CanBeNull] object value)
        {
            var memberExpr = lambdaExpression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            foreach (var validation in memberExpr.Member.GetCustomAttributes<ValidationAttribute>())
            {
                validation.Validate(value, $"Setting '{lambdaExpression.CreateName().ToString()}' is not valid.");
            }
        }

        internal static object Select([NotNull] this LambdaExpression lambdaExpression)
        {
            if (lambdaExpression == null) { throw new ArgumentNullException(nameof(lambdaExpression)); }
            if (lambdaExpression.Body is MemberExpression memberExpression)
            {
                var obj = memberExpression.GetObject();

                switch (memberExpression.Member.MemberType)
                {
                    case MemberTypes.Property:
                        var property = (PropertyInfo)memberExpression.Member;
                        return property.GetValue(obj);
                    case MemberTypes.Field:
                        return ((FieldInfo)memberExpression.Member).GetValue(obj);
                    default:
                        throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
                }
            }
            else
            {
                throw new ArgumentException($"Expression must be a {nameof(MemberExpression)}.");
            }
        }

        // Gets the object this member belongs to or null if it's in a static class.
        internal static object GetObject(this MemberExpression memberExpression)
        {
            switch (memberExpression.Expression)
            {
                case null:
                    // This is a static class.
                    return null;

                case MemberExpression anonymousMemberExpression:
                    // Extract constant value from the anonyous-wrapper
                    var container = ((ConstantExpression)anonymousMemberExpression.Expression).Value;
                    return ((FieldInfo)anonymousMemberExpression.Member).GetValue(container);

                case ConstantExpression constantExpression:
                    return constantExpression.Value;

                default:
                    throw new ArgumentException($"Expression '{memberExpression.Expression.GetType().Name}' is not supported.");
            }
        }
    }
}