using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class SettingProxy
    {
        private readonly ContainerPath _containerPath;
        private readonly object _container;
        private readonly PropertyInfo _property;
        private readonly IImmutableSet<IDatastore> _stores;
        private readonly TypeConverter _converter;
        private IDatastore _currentStore;

        public SettingProxy(object container, ContainerPath containerPath, PropertyInfo property, IImmutableSet<IDatastore> stores, TypeConverter converter)
        {
            _container = container;
            _containerPath = containerPath;
            _property = property;
            _stores = stores;
            _converter = converter;
        }

        public IEnumerable<ValidationAttribute> Validations => _property.GetCustomAttributes<ValidationAttribute>();

        public bool IsItemized => _property.GetCustomAttribute<ItemizedAttribute>().IsNotNull();

        public FormatAttribute Format => _property.GetCustomAttribute<FormatAttribute>();

        public Type Type => _property.PropertyType;

        public SettingPath Path => SettingPath.Create(_containerPath, _property, string.Empty);

        private object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }

        #region Loading

        public Result Load(LoadOption loadOption)
        {

            if (loadOption == LoadOption.Update && _currentStore.IsNotNull())
            {
                return Try.Execute(Update);
            }
            else
            {
                return Try.Execute(Resolve);
            }
        }

        private Result Update()
        {
            var value = Load(_currentStore);
            if (value)
            {
                Value = value.Value;
                return Result.Ok();
            }
            else
            {
                return Result.Fail($"'{Path.ToFullWeakString()}' not be updasted.");
            }
        }

        private Result Resolve()
        {
            // Try to load the setting with each datastore and pick the first one that succeeded.
            var result =
                (from store in _stores
                 let value = Load(store)
                 where value.Succees
                 select new { value.Value, store }).FirstOrDefault();

            if (result == null)
            {
                return Result.Fail($"'{Path.ToFullWeakString()}' not found in any datastore.");
            }

            _currentStore = result.store;
            Value = result.Value;
            return Result.Ok();
        }

        private Result<object> Load(IDatastore store)
        {
            var settings = store.Read(Path);
            if (!settings) return Result<object>.Fail($"'{Path.ToFullWeakString()}' not found in '{store.Name}'.");

            var data = GetData(settings.AsEnumerable<ISetting>());
            if (!data) return Result<object>.Fail(data.Message);

            var value = data.Value == null ? null : _converter.Convert(data.Value, Type, Format?.FormatString, Format?.FormatProvider ?? CultureInfo.InvariantCulture);

            foreach (var validation in Validations)
            {
                try
                {
                    validation.Validate(value, Path.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance));
                }
                catch (ValidationException ex) { return Result.Fail(ex, $"'{Path.ToFullWeakString()}' failed validation."); }
                catch (Exception ex) { return Result.Fail(ex, $"'{Path.ToFullWeakString()}' raised an unexpected validation error."); }
            }

            return Result<object>.Conditional(
                () => value != null,
                () => value,
                () => $"'{Path.ToFullWeakString()}' is null.");
        }

        private Result<object> GetData(IEnumerable<ISetting> settings)
        {
            switch (IsItemized)
            {
                case true when Type.IsDictionary(): return Result<object>.Ok(settings.ToDictionary(x => x.Path.ElementName, x => x.Value));
                case true when Type.IsEnumerable(): return Result<object>.Ok(settings.Select(x => x.Value));
                case true: return Result<object>.Fail($"'{Path.ToFullWeakString()}' uses a type '{Type}' that is not supported for itemized settings.");
                default: return settings.SingleOrDefault()?.Value;
            }
        }

        #endregion

        public Result Save()
        {
            return Result.Ok();
        }
    }
}