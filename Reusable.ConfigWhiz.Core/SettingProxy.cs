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

        public SettingPath Path => SettingPath.Create(_containerPath, _property, string.Empty);

        private object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }

        public Result Load(LoadOption loadOption)
        {
            var sw = Stopwatch.StartNew();

            // Try to load the setting with each datastore.
            foreach (var store in _stores)
            {
                var settings = store.Read(Path);
                if (!settings) { continue; }

                var data = GetData(settings.AsEnumerable<ISetting>());
                if (!data) return Result.Fail(data.Message, sw.Elapsed);
                var value = data.Value == null ? null : _converter.Convert(data.Value, Type, Format?.FormatString, Format?.FormatProvider ?? CultureInfo.InvariantCulture);

                foreach (var validation in Validations)
                {
                    try
                    {
                        validation.Validate(value, Path.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance));
                    }
                    catch (ValidationException ex) { return Result.Fail(ex, $"'{Path.ToFullWeakString()}' failed validation.", sw.Elapsed); }
                    catch (Exception ex) { return Result.Fail(ex, $"'{Path.ToFullWeakString()}' raised an unexpected validation error.", sw.Elapsed); }
                }

                if (value == null) return Result.Fail($"'{Path.ToFullWeakString()}' is null.", sw.Elapsed);

                _currentStore = store;
                Value = value;
                return Result.Ok(sw.Elapsed);
            }

            return Result.Fail($"'{Path.ToFullWeakString()}' not found.", sw.Elapsed);

            Result<object> GetData(IEnumerable<ISetting> settings)
            {
                switch (IsItemized)
                {
                    case true when Type.IsDictionary(): return Result<object>.Ok(settings.ToDictionary(x => x.Path.ElementName, x => x.Value));
                    case true when Type.IsEnumerable(): return Result<object>.Ok(settings.Select(x => x.Value));
                    case true: return Result<object>.Fail($"'{Path.ToFullWeakString()}' uses a type '{Type}' that is not supported for itemized settings.");
                    default: return settings.SingleOrDefault()?.Value;
                }
            }
        }

        public Result Save()
        {
            return Result.Ok();
        }
    }
}