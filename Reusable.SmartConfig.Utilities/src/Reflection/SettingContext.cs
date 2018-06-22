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
    public class SettingContext
    {
        private static readonly IDataFuse<LambdaExpression> SettingExpressionValidator =
            DataFuse
                .For<LambdaExpression>()
                .IsNotValidWhen(expression => expression == null, DataFuseOptions.StopOnFailure)
                .IsValidWhen(expression => expression.Body is MemberExpression);

        [NotNull]
        private readonly LambdaExpression _expression;

        public SettingContext([NotNull] LambdaExpression expression, [CanBeNull] string instance, bool nonPublic = false)
        {
            expression
                .ValidateWith(SettingExpressionValidator)
                .ThrowIfNotValid();

            _expression = expression;

            (ClassType, ClassInstance) = ClassFinder.FindClass(expression, nonPublic);

            var memberExpression = (MemberExpression)_expression.Body;

            Attributes = memberExpression.Member.GetCustomAttributes();

            SettingName = new SettingName(CustomSettingName ?? memberExpression.Member.Name)
            {
                Namespace = ClassType.Namespace,
                Type = ClassType.ToPrettyString(),
                Instance = instance
            };
        }

        [NotNull]
        public Type ClassType { get; }

        [CanBeNull]
        public object ClassInstance { get; }

        [NotNull]
        public SettingName SettingName { get; }

        [CanBeNull]
        public string ProviderName => Attributes.OfType<SmartSettingAttribute>().SingleOrDefault()?.DataStoreName;

        [CanBeNull]
        public string CustomSettingName => Attributes.OfType<SmartSettingAttribute>().SingleOrDefault()?.Name;

        [CanBeNull]
        public object DefaultValue => Attributes.OfType<DefaultValueAttribute>().SingleOrDefault()?.Value;

        [NotNull, ItemNotNull]
        public IEnumerable<ValidationAttribute> Validations => Attributes.OfType<ValidationAttribute>();

        [NotNull, ItemNotNull]
        public IEnumerable<Attribute> Attributes { get; }

        [CanBeNull]
        public object GetValue()
        {
            var member = ((MemberExpression)_expression.Body).Member;

            if (member is PropertyInfo property)
            {
                return property.GetValue(ClassInstance);
            }

            if (member is FieldInfo field)
            {
                return field.GetValue(ClassInstance);
            }

            throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
        }

        public void SetValue([CanBeNull] object value)
        {
            var member = ((MemberExpression)_expression.Body).Member;

            if (member is PropertyInfo property)
            {
                if (property.CanWrite)
                {
                    property.SetValue(ClassInstance, value);
                }
                // This is a readonly property. We try to write directly to the backing-field.
                else
                {
                    var bindingFlags = BindingFlags.NonPublic | (ClassInstance == null ? BindingFlags.Static : BindingFlags.Instance);
                    var backingField = ClassType.GetField($"<{property.Name}>k__BackingField", bindingFlags);
                    if (backingField is null)
                    {
                        throw ("BackingFieldNotFoundException", $"Property {property.Name.QuoteWith("'")} does not have a default backing field.").ToDynamicException();
                    }

                    backingField.SetValue(ClassInstance, value);
                }
                return;
            }

            if (member is FieldInfo field)
            {

                field.SetValue(ClassInstance, value);
                return;
            }

            throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
        }
    }
}