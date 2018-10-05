using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.Validation;

namespace Reusable.SmartConfig.Reflection
{
    [PublicAPI]
    public class SettingInfo
    {
        private static readonly IDuckValidator<LambdaExpression> SettingExpressionValidator = new DuckValidator<LambdaExpression>(expression =>
        {
            expression
                .IsNotValidWhenNull()
                .IsValidWhen(e => e.Body is MemberExpression);
        });

        //[NotNull]
        //private readonly LambdaExpression _expression;

        //public SettingInfo([NotNull] LambdaExpression expression, [CanBeNull] string instance, bool nonPublic = false)
        //{
        //    expression
        //        .ValidateWith(SettingExpressionValidator)
        //        .ThrowIfNotValid();

        //    _expression = expression;

        //    (Type, Instance) = ClassFinder.FindClass(expression, nonPublic);

        //    var memberExpression = (MemberExpression)_expression.Body;

        //    Attributes = memberExpression.Member.GetCustomAttributes();

        //    SettingName = new SettingName(CustomSettingName ?? memberExpression.Member.Name)
        //    {
        //        Namespace = Type.Namespace,
        //        Type = Type.ToPrettyString(),
        //        Instance = instance
        //    };
        //}

        private SettingInfo(Type type, object instance, MemberInfo member)
        {
            Type = type;
            Instance = instance;
            Member = member;

            var attributes = new SettingAttribute[]
            {
                type.Assembly.GetCustomAttribute<SettingPrefixAttribute>(),
                type.GetCustomAttribute<SettingTypeAttribute>(),
                member.GetCustomAttribute<SettingPrefixAttribute>(),
            };
            
            var prefixHandling = attributes.Select(a => a?.PrefixHandling).LastOrDefault(x=> x.HasValue) ?? PrefixHandling.Inherit; 

            Prefix = 
                prefixHandling == PrefixHandling.Inherit 
            ? attributes.Select(a => a?.Name).Last(Conditional.IsNotNullOrEmpty)
                    : 
            Schema = type.Namespace;
            TypeName = member.GetCustomAttribute<SettingTypeAttribute>()?.Name ?? type.ToPrettyString();
            MemberName = member.GetCustomAttribute<SettingPrefixAttribute>()?.Name ?? member.Name;
            
            ProviderName = attributes.Select(a => a?.ProviderName).LastOrDefault(Conditional.IsNotNullOrEmpty);
            SettingNameComplexity = attributes.Select(a => a?.Complexity).LastOrDefault(Conditional.IsNotNull);
            PrefixHandlingEnabled = attributes.Select(a => a.PrefixEnabled).LastOrDefault(Conditional.IsNotNull);
            DefaultValue = member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            Validations = member.GetCustomAttributes<ValidationAttribute>();
        }

        [NotNull]
        public Type Type { get; }

        [CanBeNull]
        public object Instance { get; }

        [NotNull]
        private MemberInfo Member { get; }

        [CanBeNull]
        public string Prefix { get; }

        [CanBeNull]
        public string Schema { get; }

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

        public SettingNameComplexity SettingNameComplexity { get; }

        public PrefixHandling PrefixHandling { get; }

        public static SettingInfo FromExpression(LambdaExpression expression, bool nonPublic)
        {
            var (type, instance, member) = SettingVisitor.GetSettingInfo(expression, nonPublic);
            return new SettingInfo(type, instance, member);
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
                            throw ("BackingFieldNotFoundException", $"Property {property.Name.QuoteWith("'")} does not have a default backing field.").ToDynamicException();
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