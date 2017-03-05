using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable.TypeConversion;
using Reusable.Shelly.Collections;
using System.Collections.Immutable;

namespace Reusable.Shelly
{
    internal class ParameterFactory
    {
        private readonly TypeConverter _converter;
        private readonly ParameterCollection _parameters;

        public ParameterFactory(Type parameterType)
        {
            _parameters = new ParameterCollection(parameterType);
            _converter = DefaultConverter;
        }

        public object CreateParameter(ArgumentCollection arguments)
        {
            if (!_parameters.Any()) return null;

            var instance = Activator.CreateInstance(_parameters.ParameterType);
            //var parameterProperties = parameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttribute<ParameterAttribute>() != null);

            foreach (var parameter in _parameters)
            {
                var arg = arguments[parameter.Names] ?? arguments[parameter.Position];
                
                if (parameter.Required && arg == null) throw new ParameterNotFoundException();               
            }

            return null;

        }

        private TypeConverter DefaultConverter => TypeConverter.Empty.Add(new TypeConverter[]
        {
            new StringToSByteConverter(),
            new StringToByteConverter(),
            new StringToCharConverter(),
            new StringToInt16Converter(),
            new StringToInt32Converter(),
            new StringToInt64Converter(),
            new StringToUInt16Converter(),
            new StringToUInt32Converter(),
            new StringToUInt64Converter(),
            new StringToSingleConverter(),
            new StringToDoubleConverter(),
            new StringToDecimalConverter(),
            new StringToColorConverter(),
            new StringToBooleanConverter(),
            new StringToDateTimeConverter(),
            new StringToEnumConverter(),

            //new EnumerableObjectToArrayObjectConverter(),
            //new EnumerableObjectToListObjectConverter(),
            //new EnumerableObjectToHashSetObjectConverter(),
            //new DictionaryObjectObjectToDictionaryObjectObjectConverter(),
        });
    }
}
