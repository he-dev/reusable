using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reusable.Windows
{
    public class DependencyPropertyBuilder<T, TValue> where T : DependencyObject
    {
        private readonly string _name;
        private readonly Type _propertyType;
        private readonly Type _ownerType;
        private PropertyMetadata _propertyMetadata;
        private ValidateValueCallback _validateValueCallback;

        public DependencyPropertyBuilder(
            string name,
            Type propertyType,
            Type ownerType
        )
        {
            _name = name;
            _propertyType = propertyType;
            _ownerType = ownerType;
            _propertyMetadata = new PropertyMetadata();
        }

        public DependencyPropertyBuilder<T, TValue> Metadata(
            PropertyMetadata propertyMetadata
        )
        {
            _propertyMetadata = propertyMetadata;
            return this;
        }

        public DependencyPropertyBuilder<T, TValue> ValidateValue(
            Func<TValue, bool> validateValueCallback
        )
        {
            _validateValueCallback = value => validateValueCallback((TValue)value);
            return this;
        }

        public DependencyProperty Build()
        {
            BuildDefaultValue();
            BuildValidateValueCallback();

            return DependencyProperty.Register(
                _name,
                _propertyType,
                _ownerType,
                _propertyMetadata,
                _validateValueCallback
            );
        }

        private void BuildDefaultValue()
        {
            var property = _ownerType.GetProperty(_name);

            // Use the default value specified by the user or try to use the attribute.
            _propertyMetadata.DefaultValue =
                _propertyMetadata.DefaultValue ??
                new Func<object>(() =>
                    // Get the defualt value from the attribute...
                    property.GetCustomAttribute<DefaultValueAttribute>()?.Value ??
                    // or use the default value for the type.
                    (property.PropertyType.IsValueType
                        ? Activator.CreateInstance(property.PropertyType)
                        : null
                    )
                )();
        }

        private void BuildValidateValueCallback()
        {
            var property = _ownerType.GetProperty(_name);

            // Use the callback specified by the user or try to use the attributes.
            _validateValueCallback =
                _validateValueCallback ??
                (value =>
                    new Func<bool>(() => (
                        property.GetCustomAttributes<ValidationAttribute>() ??
                        Enumerable.Empty<ValidationAttribute>()
                    ).All(x => x.IsValid(value)))()
                );
        }

        public static implicit operator DependencyProperty(DependencyPropertyBuilder<T, TValue> builder)
            => builder.Build();
    }

    public class DependencyPropertyBuilder
    {
        public static DependencyPropertyBuilder<T, TValue> Register<T, TValue>(
            string propertyName
        ) where T : DependencyObject
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);

            return new DependencyPropertyBuilder<T, TValue>(
                name: propertyName,
                propertyType: propertyInfo.PropertyType,
                ownerType: propertyInfo.DeclaringType
            );
        }

        public static DependencyPropertyBuilder<T, TValue> Register<T, TValue>(
            Expression<Func<TValue>> expression
        ) where T : DependencyObject
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(
                    paramName: nameof(expression),
                    message: "You need to provide a member expression."
                );
            }

            return new DependencyPropertyBuilder<T, TValue>(
                name: memberExpression.Member.Name,
                propertyType: ((PropertyInfo)memberExpression.Member).PropertyType,
                ownerType: ((PropertyInfo)memberExpression.Member).DeclaringType
            );
        }
    }
}
