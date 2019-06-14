using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.SmartConfig
{
    public static class ConfigurationExtensions
    {
        #region Getters

        [Obsolete("Use GetItem")]
        public static T GetSetting<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, string index = default)
        {
            return (T)configuration.GetItemAsync(expression).GetAwaiter().GetResult();
        }

        public static async Task<TValue> GetItemAsync<TValue>(this IConfiguration configuration, Expression<Func<TValue>> selector, string index = default)
        {
            return (TValue)await configuration.GetItemAsync(CreateSelector<TValue>(selector, index));
        }

        public static async Task<TValue> GetItemAsync<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> selector, string index = default)
        {
            return (TValue)await configuration.GetItemAsync(CreateSelector<TValue>(selector, index));
        }

        public static T GetItem<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> selector, string index = default)
        {
            return (T)configuration.GetItemAsync(CreateSelector<T>(selector, index)).GetAwaiter().GetResult();
        }

        public static TValue GetItem<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> selector, string index = default)
        {
            return (TValue)configuration.GetItemAsync(selector).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        [Obsolete("Use SetItem")]
        public static void SaveSetting<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string index = default)
        {
            configuration.SetItemAsync(CreateSelector<T>(selector, index), newValue).GetAwaiter().GetResult();
        }

        public static async Task SetItemAsync<TValue>(this IConfiguration configuration, Expression<Func<TValue>> selector, TValue newValue, string index = default)
        {
            await configuration.SetItemAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static async Task SetItemAsync<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> selector, TValue newValue, string index = default)
        {
            await configuration.SetItemAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void SetItem<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string index = default)
        {
            configuration.SetItemAsync(CreateSelector<T>(selector, index), newValue).GetAwaiter().GetResult();
        }

        public static void SetItem<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> selector, TValue newValue, string index = default)
        {
            configuration.SetItemAsync(CreateSelector<TValue>(selector, index), newValue).GetAwaiter().GetResult();
        }

        #endregion

        private static Selector CreateSelector<T>(LambdaExpression selector, string index)
        {
            return
                index is null
                    ? new Selector<T>(selector)
                    : new Selector<T>(selector).Index(index);
        }

        //        public static T GetSetting<T>(this IConfiguration configuration, string name, string instance = default)
        //        {
        //            var expression =
        //                Expression.Lambda<Func<T>>(
        //                    Expression.Property(
        //                        Expression.Constant(default(T), typeof(T)),
        //                        name
        //                    )
        //                );
        //            return configuration.GetSetting(expression, instance);
        //        }

        //        /// <summary>
        //        /// Assigns the same setting value to the specified member.
        //        /// </summary>
        //        public void BindSetting<T>(Expression<Func<T>> expression, string instanceName = null)
        //        {
        //            if (expression == null) throw new ArgumentNullException(nameof(expression));
        //
        //            var settingMetadata = SettingMetadata.FromExpression(expression, false);
        //            var uri = settingMetadata.CreateUri(instanceName);
        //            var value = GetSetting(expression, instanceName);
        //            settingMetadata.SetValue(Validate(value, settingMetadata.Validations, uri));
        //        }
        //
        //        /// <summary>
        //        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        //        /// </summary>        
        //        public void BindSettings<T>(T obj, string instanceName = null)
        //        {
        //            if (obj == null) throw new ArgumentNullException(nameof(obj));
        //
        //            var settingProperties =
        //                typeof(T)
        //                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        //                    .Where(p => p.IsDefined(typeof(SettingMemberAttribute)));
        //
        //            foreach (var property in settingProperties)
        //            {
        //                // This expression allows to reuse GeAsync.
        //                var expression = Expression.Lambda(
        //                    Expression.Property(
        //                        Expression.Constant(obj),
        //                        property.Name
        //                    )
        //                );
        //
        //                var value = GetSetting(expression, instanceName);
        //                var settingMetadata = SettingMetadata.FromExpression(expression, false);
        //                var uri = settingMetadata.CreateUri(instanceName);
        //                settingMetadata.SetValue(Validate(value, settingMetadata.Validations, uri));
        //            }
        //        }
    }
}