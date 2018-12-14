using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Reflection
{
    [PublicAPI]
    public class SettingMetadata
    {
        private static readonly IExpressValidator<LambdaExpression> SettingExpressionValidator = ExpressValidator.For<LambdaExpression>(builder =>
        {
            builder.BlockNull();
            builder.Ensure(e => e.Body is MemberExpression);
        });

        private SettingMetadata(Type type, object instance, MemberInfo member)
        {
            Type = type;
            Instance = instance;
            Member = member;
            MemberType = GetMemberType(member);

            //var asdf1 = member.GetCustomAttributes<SettingMemberAttribute>(inherit: true);
            //var asdf2 = member.GetCustomAttributes<SettingMemberAttribute>(inherit: false);
            
            var attributes =
                new SettingAttribute[]
                    {
                        member.GetCustomAttributes<SettingMemberAttribute>(inherit: true).FirstOrDefault(),
                        type.GetCustomAttribute<SettingTypeAttribute>(),
                    }
                    .Where(Conditional.IsNotNull)
                    .ToList();

            SettingNameStrength = attributes.FirstOrDefault(x => x.Strength != SettingNameStrength.Inherit)?.Strength ?? SettingNameStrength.Inherit;
            Prefix = attributes.Select(x => x.Prefix).FirstOrDefault(Conditional.IsNotNullOrEmpty);
            PrefixHandling = attributes.FirstOrDefault(x => x.PrefixHandling != PrefixHandling.Inherit)?.PrefixHandling ?? PrefixHandling.Inherit;

            Namespace = type.Namespace;
            TypeName = type.GetCustomAttribute<SettingTypeAttribute>()?.Name ?? type.ToPrettyString();
            MemberName = member.GetCustomAttribute<SettingMemberAttribute>()?.Name ?? member.Name;

            ProviderName = attributes.Select(x => x.ProviderName).FirstOrDefault(Conditional.IsNotNullOrEmpty);
            ProviderType = attributes.Select(x => x.ProviderType).FirstOrDefault(Conditional.IsNotNull);
            DefaultValue = member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            Validations = member.GetCustomAttributes<ValidationAttribute>();
        }

        [NotNull]
        public Type Type { get; }

        [CanBeNull]
        public object Instance { get; }

        [NotNull]
        public MemberInfo Member { get; }
        
        [NotNull]
        public Type MemberType { get; }

        [CanBeNull]
        public string Prefix { get; }

        [CanBeNull]
        public string Namespace { get; }

        [CanBeNull]
        public string TypeName { get; }

        [NotNull]
        public string MemberName { get; }

        [NotNull, ItemNotNull]
        public IEnumerable<ValidationAttribute> Validations { get; }

        [CanBeNull]
        public object DefaultValue { get; }

        [CanBeNull]
        public string ProviderName { get; }
        
        [CanBeNull]
        public Type ProviderType { get; }

        public SettingNameStrength SettingNameStrength { get; }

        public PrefixHandling PrefixHandling { get; }

        [NotNull]
        public static SettingMetadata FromExpression(LambdaExpression expression, bool nonPublic = false)
        {
            expression.ValidateWith(SettingExpressionValidator).ThrowIfInvalid();

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
        public object GetValue()
        {
            switch (Member)
            {
                case PropertyInfo property:
                    return property.GetValue(Instance);

                case FieldInfo field:
                    return field.GetValue(Instance);

                default:
                    throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
            }
        }

        public void SetValue([CanBeNull] object value)
        {
            switch (Member)
            {
                case PropertyInfo property:
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(Instance, value);
                    }
                    // This is a readonly property. We try to write directly to the backing-field.
                    else
                    {
                        var bindingFlags = BindingFlags.NonPublic | (Instance == null ? BindingFlags.Static : BindingFlags.Instance);
                        var backingField = Type.GetField($"<{property.Name}>k__BackingField", bindingFlags);
                        if (backingField is null)
                        {
                            throw ("BackingFieldNotFound", $"Property {property.Name.QuoteWith("'")} does not have a default backing field.").ToDynamicException();
                        }

                        backingField.SetValue(Instance, value);
                    }
                }
                    break;

                case FieldInfo field:
                {
                    field.SetValue(Instance, value);
                }
                    break;

                default:
                    throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
            }
        }

        //public override string ToString() => SettingName.ToString();
    }
}