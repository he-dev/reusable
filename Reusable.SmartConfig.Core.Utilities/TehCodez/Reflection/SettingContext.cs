using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.SmartConfig.Data;
using Validator = Reusable.Flawless.Validator;

namespace Reusable.SmartConfig.Utilities.Reflection
{
    public class SettingContext
    {
        private static readonly IValidator<LambdaExpression> LambdaExpressionValidator =
            Validator
                .Create<LambdaExpression>()
                .IsNotValidWhen(expression => expression == null, ValidationOptions.StopOnFailure)
                .IsValidWhen(expression => expression.Body is MemberExpression);

        [NotNull]
        private readonly LambdaExpression _expression;

        public SettingContext([NotNull] LambdaExpression expression, [CanBeNull] string instance, bool nonPublic = false)
        {
            expression
                .ValidateWith(LambdaExpressionValidator)
                .ThrowIfNotValid();

            _expression = expression;

            (OwnerType, Object) = ClassFinder.FindClass(expression, nonPublic);

            var memberExpression = (MemberExpression)_expression.Body;

            Attributes = memberExpression.Member.GetCustomAttributes();

            SettingName = new SettingName(CustomSettingName ?? memberExpression.Member.Name)
            {
                Namespace = OwnerType.Namespace,
                Type = OwnerType.Name,
                Instance = instance
            };
        }

        [NotNull]
        public Type OwnerType { get; }

        [CanBeNull]
        public object Object { get; }

        [NotNull]
        public SettingName SettingName { get; }

        [CanBeNull]
        public string DataStoreName => Attributes.OfType<SmartSettingAttribute>().SingleOrDefault()?.DataStoreName;

        [CanBeNull]
        public string CustomSettingName => Attributes.OfType<SmartSettingAttribute>().SingleOrDefault()?.Name;

        //[CanBeNull]
        //public SmartSettingAttribute Options => Attributes.OfType<SmartSettingAttribute>().SingleOrDefault();

        [CanBeNull]
        public object DefaultValue=> Attributes.OfType<DefaultValueAttribute>().SingleOrDefault()?.Value;

        [NotNull, ItemNotNull]
        public IEnumerable<ValidationAttribute> Validations => Attributes.OfType<ValidationAttribute>();

        [NotNull, ItemNotNull]
        public IEnumerable<Attribute> Attributes { get; }

        public object GetValue()
        {
            var memberExpression = (MemberExpression)_expression.Body;

            switch (memberExpression.Member.MemberType)
            {
                case MemberTypes.Property:
                    var property = (PropertyInfo)memberExpression.Member;
                    return property.GetValue(Object);

                case MemberTypes.Field:
                    return ((FieldInfo)memberExpression.Member).GetValue(Object);

                default:
                    throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
            }
        }

        public void SetValue([CanBeNull] object value)
        {
            var member = ((MemberExpression)_expression.Body).Member;

            if (member is PropertyInfo property)
            {
                if (property.CanWrite)
                {
                    property.SetValue(Object, value);
                }
                // This is a readonly property. We try to write directly to the backing-field.
                else
                {
                    var bindingFlags = BindingFlags.NonPublic | (Object == null ? BindingFlags.Static : BindingFlags.Instance);
                    var backingField = OwnerType.GetField($"<{property.Name}>k__BackingField", bindingFlags);
                    if (backingField is null)
                    {
                        throw ("BackingFieldNotFoundException", $"Property {property.Name.QuoteWith("'")} does not have a default backing field.").ToDynamicException();
                    }

                    backingField.SetValue(Object, value);
                }
                return;
            }

            if (member is FieldInfo field)
            {

                field.SetValue(Object, value);
                return;
            }

            throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
        }
    }
}