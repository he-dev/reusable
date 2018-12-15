using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Reflection;
using Reusable.Stratus;

namespace Reusable.SmartConfig
{
    public static class ValueProviderExtensions
    {
        #region GetValue overloads

        [ItemNotNull]
        public static async Task<T> GetSettingAsync<T>([NotNull] this IValueProvider valueProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)await valueProvider.GetSettingAsync((LambdaExpression)expression, instanceName);
        }

        [ItemNotNull]
        private static async Task<object> GetSettingAsync([NotNull] this IValueProvider valueProvider, [NotNull] LambdaExpression expression, [CanBeNull] string instanceName = null)
        {
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var settingName =
                settingMetadata
                    .CreateSettingName(instanceName)
                    .ModifySettingName
                    (
                        settingMetadata.SettingNameStrength,
                        settingMetadata.Prefix,
                        settingMetadata.PrefixHandling
                    );

            var settingInfo =
                await
                    valueProvider
                        .GetValueInfoAsync
                        (
                            settingName,
                            ValueProviderMetadata.Empty
                                .Add(ValueProviderMetadataKeyNames.ProviderName, settingMetadata.ProviderName)
                        );

            if (settingInfo.Exists)
            {
                var value = (await settingInfo.DeserializeAsync(settingMetadata.MemberType)) ?? settingMetadata.DefaultValue;
                return
                    settingMetadata
                        .Validations
                        .Validate(settingName, value);
            }

            return default;
        }

        #endregion

        #region SetValue overloads

        [ItemNotNull]
        public static async Task<IValueProvider> SetSettingAsync<T>([NotNull] this IValueProvider valueProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);

            var settingName =
                settingMetadata
                    .CreateSettingName(instanceName)
                    .ModifySettingName
                    (
                        settingMetadata.SettingNameStrength,
                        settingMetadata.Prefix,
                        settingMetadata.PrefixHandling
                    );

            var settingInfo =
                await
                    valueProvider
                        .GetValueInfoAsync
                        (
                            settingName,
                            ValueProviderMetadata.Empty
                                .Add(ValueProviderMetadataKeyNames.ProviderName, settingMetadata.ProviderName)
                        );

            if (settingInfo.Exists)
            {
                settingMetadata
                    .Validations
                    .Validate
                    (
                        settingName,
                        newValue
                    );

                await valueProvider.SerializeAsync(settingName, newValue);
            }

            return valueProvider;
        }

        #endregion

        #region AssignValue overloads

        /// <summary>
        /// Assigns the same setting value to the specified member.
        /// </summary>
        [ItemNotNull]
        public static async Task<IValueProvider> BindSettingAsync<T>([NotNull] this IValueProvider valueProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var settingName =
                settingMetadata
                    .CreateSettingName(instanceName)
                    .ModifySettingName
                    (
                        settingMetadata.SettingNameStrength,
                        settingMetadata.Prefix,
                        settingMetadata.PrefixHandling
                    );

            var value = await valueProvider.GetSettingAsync(expression, instanceName);

            settingMetadata
                .Validations
                .Validate(settingName, value);

            settingMetadata.SetValue(value);

            return valueProvider;
        }

        /// <summary>
        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        /// </summary>
        [ItemNotNull]
        public static async Task<IValueProvider> BindSettingsAsync<T>([NotNull] this IValueProvider valueProvider, [NotNull] T obj, [CanBeNull] string instanceName = null)
        {
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var settingProperties =
                typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.IsDefined(typeof(SettingMemberAttribute)));

            foreach (var property in settingProperties)
            {
                // This expression allows to reuse GeAsync.
                var expression = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(obj),
                        property.Name
                    )
                );

                var value = await valueProvider.GetSettingAsync(expression, instanceName);
                var settingMetadata = SettingMetadata.FromExpression(expression, false);
                var settingName =
                    settingMetadata
                        .CreateSettingName(instanceName)
                        .ModifySettingName
                        (
                            settingMetadata.SettingNameStrength,
                            settingMetadata.Prefix,
                            settingMetadata.PrefixHandling
                        );
                settingMetadata
                    .Validations
                    .Validate(settingName, value);
                settingMetadata.SetValue(value);

            }

            return valueProvider;
        }

        #endregion

        private static object Validate(this IEnumerable<ValidationAttribute> validations, SettingName settingName, object value)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, $"Setting {settingName.ToString().QuoteWith("'")} is not valid.");
            }

            return value;
        }
    }
}