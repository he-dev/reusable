using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    public static class ConfigurationExtensions
    {
        public static async Task<TValue> GetItemAsync<TValue>(this IConfiguration configuration, Expression<Func<TValue>> getItem, string handle = default)
        {
            return (TValue)await configuration.GetItemAsync(getItem, handle);
        }

        public static async Task SetItemAsync<TValue>(this IConfiguration configuration, Expression<Func<TValue>> setItem, TValue newValue, string handle = default)
        {
            await configuration.SetItemAsync(setItem, newValue, handle);
        }

        [Obsolete("Use GetItem")]
        public static T GetSetting<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] string handle = null)
        {
            return (T)configuration.GetItemAsync(expression, handle).GetAwaiter().GetResult();
        }

        [Obsolete("Use SetItem")]
        public static void SaveSetting<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string handle = null)
        {
            configuration.SetItemAsync(expression, newValue, handle).GetAwaiter().GetResult();
        }

        public static T GetItem<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] string handle = null)
        {
            return (T)configuration.GetItemAsync(expression, handle).GetAwaiter().GetResult();
        }

        public static void SetItem<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string handle = null)
        {
            configuration.SetItemAsync(expression, newValue, handle).GetAwaiter().GetResult();
        }

        public static async Task<TValue> GetItemAsync<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> getItem, string handle = default)
        {
            return (TValue)await configuration.GetItemAsync(getItem, handle);
        }
        
        public static TValue GetItem<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> getItem, string handle = default)
        {
            return (TValue)configuration.GetItemAsync(getItem, handle).GetAwaiter().GetResult();
        }

        public static async Task SetItemAsync<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> setItem, TValue newValue, string handle = default)
        {
            await configuration.SetItemAsync(setItem, newValue, handle);
        }
        
        public static void SetItem<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> setItem, TValue newValue, string handle = default)
        {
            configuration.SetItemAsync(setItem, newValue, handle).GetAwaiter().GetResult();
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