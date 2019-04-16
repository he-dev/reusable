using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public class SettingMetadata
    {
        private static readonly IExpressValidator<LambdaExpression> SettingExpressionValidator = ExpressValidator.For<LambdaExpression>(builder =>
        {
            builder.NotNull();
            builder.True(e => e.Body is MemberExpression);
        });

        private SettingMetadata(Type type, object typeInstance, MemberInfo member)
        {
            Scope = type.Namespace;
            Type = type;
            TypeName =
                GetCustomAttribute<ResourceNameAttribute>(type, default)?.Name ??
                (
                    type.IsInterface
                        ? Regex.Match(type.Name, @"^I(?<name>\w+)(?:Config(uration)?)", RegexOptions.IgnoreCase).Group("name")
                        : type.Name
                );
            TypeInstance = typeInstance;
            Member = member;
            MemberName = GetCustomAttribute<ResourceNameAttribute>(default, member)?.Name ?? member.Name;
            MemberType = GetMemberType(member);

            ResourceScheme = GetCustomAttribute<ResourceSchemeAttribute>(type, member)?.Name ?? "setting";
            ResourcePrefix = GetCustomAttribute<ResourceNameAttribute>(type, member)?.Name;
            ResourceProviderType = GetCustomAttribute<ResourceProviderAttribute>(type, member)?.Type;
            ResourceProviderName = GetCustomAttribute<ResourceProviderAttribute>(type, member)?.Name;
            Validations = member.GetCustomAttributes<ValidationAttribute>();

            DefaultValue = member.GetCustomAttribute<DefaultValueAttribute>()?.Value;

            Level = GetCustomAttribute<ResourceNameAttribute>(type, member)?.Level ?? ResourceNameLevel.TypeMember;
        }

        [NotNull]
        public string Scope { get; }

        [NotNull]
        public Type Type { get; }

        [CanBeNull]
        public string TypeName { get; }

        [CanBeNull]
        public object TypeInstance { get; }

        [NotNull]
        public MemberInfo Member { get; }

        [NotNull]
        public string MemberName { get; }

        [NotNull]
        public Type MemberType { get; }

        [NotNull]
        public string ResourceScheme { get; }

        [CanBeNull]
        public string ResourcePrefix { get; }

        [CanBeNull]
        public string ResourceProviderName { get; }

        [CanBeNull]
        public Type ResourceProviderType { get; }

        [NotNull, ItemNotNull]
        public IEnumerable<ValidationAttribute> Validations { get; }

        [CanBeNull]
        public object DefaultValue { get; }

        public ResourceNameLevel Level { get; }

        [NotNull]
        public static SettingMetadata FromExpression(LambdaExpression expression, bool nonPublic = false)
        {
            expression.ValidateWith(SettingExpressionValidator).Assert();

            var (type, instance, member) = SettingVisitor.GetSettingInfo(expression, nonPublic);
            return new SettingMetadata(type, instance, member);
        }

        [NotNull]
        private Type GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo property:
                    return property.PropertyType;

                case FieldInfo field:
                    return field.FieldType;

                default:
                    throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
            }
        }

        [CanBeNull]
        private static T GetCustomAttribute<T>(Type type, MemberInfo member) where T : Attribute
        {
            return new[]
                {
                    member?.GetCustomAttributes<T>(inherit: true).FirstOrDefault(),
                    type?.GetCustomAttribute<T>(),
                }
                .FirstOrDefault(Conditional.IsNotNull);
        }
    }
}