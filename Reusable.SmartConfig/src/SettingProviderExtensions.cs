using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    public static class SettingProviderExtensions
    {
        #region GetValue overloads

        [ItemNotNull]
        public static async Task<T> GetSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)await resourceProvider.GetSettingAsync((LambdaExpression)expression, instanceName);
        }

        [NotNull]
        public static T GetSetting<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)resourceProvider.GetSettingAsync((LambdaExpression)expression, instanceName).GetAwaiter().GetResult();
        }

        [ItemNotNull]
        private static async Task<object> GetSettingAsync([NotNull] this IResourceProvider resourceProvider, [NotNull] LambdaExpression expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);
            var settingInfo =
                await
                    resourceProvider
                        .GetAsync(uri, PopulateProviderInfo(settingMetadata));

            if (settingInfo.Exists)
            {
                var value = (await settingInfo.DeserializeAsync(settingMetadata.MemberType)) ?? settingMetadata.DefaultValue;
                return value.Validate(settingMetadata.Validations, uri);
            }
            else
            {
                throw DynamicException.Create("SettingNotFound", $"Could not find '{uri}'.");
            }
        }

        #endregion

        #region SetValue overloads

        [ItemNotNull]
        public static async Task<IResourceProvider> SetSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);

            //var settingInfo =
            //    await
            //        resourceProvider
            //            .GetAsync(uri, PopulateProviderInfo(settingMetadata));

            //if (settingInfo.Exists)
            {
                //settingMetadata
                //    .Validations
                //    .Validate(settingName, newValue);

                newValue.Validate(settingMetadata.Validations, uri);
                var resource = ResourceHelper.CreateStream(newValue);
                await resourceProvider.PutAsync(uri, resource.Stream, PopulateProviderInfo(settingMetadata, resource.Metadata));
            }

            return resourceProvider;
        }

        [NotNull]
        public static IResourceProvider SetSetting<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            return resourceProvider.SetSettingAsync(expression, newValue, instanceName).GetAwaiter().GetResult();
        }

        #endregion

        #region AssignValue overloads

        /// <summary>
        /// Assigns the same setting value to the specified member.
        /// </summary>
        [ItemNotNull]
        public static async Task<IResourceProvider> BindSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);
            var value = await resourceProvider.GetSettingAsync(expression, instanceName);
            settingMetadata.SetValue(value.Validate(settingMetadata.Validations, uri));

            return resourceProvider;
        }

        /// <summary>
        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        /// </summary>
        [ItemNotNull]
        public static async Task<IResourceProvider> BindSettingsAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] T obj, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
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

                var value = await resourceProvider.GetSettingAsync(expression, instanceName);
                var settingMetadata = SettingMetadata.FromExpression(expression, false);
                var uri = settingMetadata.CreateUri(instanceName);
                settingMetadata.SetValue(value.Validate(settingMetadata.Validations, uri));

            }

            return resourceProvider;
        }

        #endregion

        private static object Validate(this object value, IEnumerable<ValidationAttribute> validations, UriString uri)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, uri);
            }

            return value;
        }

        private static ResourceMetadata PopulateProviderInfo(SettingMetadata settingMetadata, ResourceMetadata metadata = null)
        {
            return
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.ProviderCustomName, settingMetadata.ProviderName)
                    .Add(ResourceMetadataKeys.ProviderDefaultName, settingMetadata.ProviderType?.ToPrettyString());
        }
    }
}