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
        private readonly IImmutableList<IDatastore> _stores;
        private readonly TypeConverter _converter;
        private IDatastore _currentStore;

        public SettingProxy(object container, ContainerPath containerPath, PropertyInfo property, IImmutableList<IDatastore> stores, TypeConverter converter)
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

        private object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }

        public Result<SettingProxy, bool> Load(LoadOption loadOption)
        {
            var sw = Stopwatch.StartNew();

            // a.b - full-weak
            // App.Windows.MainWindow["Window1"].WindowDimensions.Height
            var path = SettingPath.Create(_containerPath, _property, null);

            foreach (var store in _stores)
            {
                var settings = store.Read(path);
                if (!settings) { continue; }

                var data = GetData(settings.AsEnumerable<ISetting>());
                if (!data) return Result<SettingProxy, bool>.Fail(this, data.Message, sw.Elapsed);
                var value = data.Value == null ? null : _converter.Convert(data.Value, Type, Format?.FormatString, Format?.FormatProvider ?? CultureInfo.InvariantCulture);

                foreach (var validation in Validations)
                {
                    try
                    {
                        validation.Validate(value, path.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance));
                    }
                    catch (ValidationException ex) { return (this, ex, "Could not validate setting.", sw.Elapsed); }
                    catch (Exception ex) { return (this, ex, "Unexpected validation error.", sw.Elapsed); }
                }

                if (value == null) return (this, false, sw.Elapsed);

                _currentStore = store;
                Value = value;
                return (this, true, sw.Elapsed);
            }

            return Result<SettingProxy, bool>.Fail(this, "Setting not found.", sw.Elapsed);

            Result<object> GetData(IEnumerable<ISetting> settings)
            {
                switch (IsItemized)
                {
                    case true when Type.IsDictionary(): return Result<object>.Ok(settings.ToDictionary(x => x.Path.ElementName, x => x.Value));
                    case true when Type.IsEnumerable(): return Result<object>.Ok(settings.Select(x => x.Value));
                    case true: return Result<object>.Fail($"Setting type '{Type}' is not supported for itemized settings.");
                    default: return settings.SingleOrDefault()?.Value;
                }
            }
        }

        public Result<SettingProxy, bool> Save()
        {
            return Result<SettingProxy, bool>.Ok(this, true);
        }
    }
}