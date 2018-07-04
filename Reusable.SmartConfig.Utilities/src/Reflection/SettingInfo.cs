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

namespace Reusable.SmartConfig.Utilities.Reflection
{
    [PublicAPI]
    public class SettingInfo
    {
        private static readonly IDataFuse<LambdaExpression> SettingExpressionValidator =
            DataFuse
                .For<LambdaExpression>()
                .IsNotValidWhen(expression => expression == null, DataFuseOptions.StopOnFailure)
                .IsValidWhen(expression => expression.Body is MemberExpression);

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

        private SettingInfo(Type type, object instance, MemberInfo member, string instanceName)
        {
            Type = type;
            Instance = instance;
            Member = member;

            var name = member.GetCustomAttribute<SmartSettingAttribute>()?.Name ?? member.Name;

            SettingName = new SettingName(name)
            {
                Namespace = Type.Namespace,
                Type = Type.ToPrettyString(),
                Instance = instanceName
            };
        }

        [NotNull]
        public Type Type { get; }

        [CanBeNull]
        public object Instance { get; }

        [NotNull]
        private MemberInfo Member { get; }

        [CanBeNull]
        private SmartSettingAttribute Attribute => Member.GetCustomAttribute<SmartSettingAttribute>();

        [NotNull, ItemNotNull]
        public IEnumerable<ValidationAttribute> Validations => Member.GetCustomAttributes<ValidationAttribute>();

        [CanBeNull]
        public object DefaultValue => Member.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        [NotNull]
        public SettingName SettingName { get; }

        [CanBeNull]
        public string ProviderName => Attribute?.ProviderName;

        [CanBeNull]
        public string CustomSettingName => Attribute?.Name;

        public static SettingInfo FromExpression(LambdaExpression expression, bool nonPublic, string key)
        {
            var (type, instance, member) = SettingVisitor.GetSettingInfo(expression, nonPublic);
            return new SettingInfo(type, instance, member, key);
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

        public override string ToString() => SettingName.ToString();
    }
}