using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable.TypeConversion;
using Reusable.Shelly.Collections;

namespace Reusable.Shelly
{
    internal class ParameterFactory
    {
        private readonly TypeConverter _converter;
        private readonly ParameterCollection _parameters;

        public ParameterFactory(Type parameterType, TypeConverter converter)
        {
            _parameters = new ParameterCollection(parameterType);
            _converter = converter;
        }

        public object CreateParameter(ArgumentCollection arguments)
        {
            //var parameter = Activator.CreateInstance(parameterType);
            //var parameterProperties = parameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttribute<ParameterAttribute>() != null);

            foreach (var parameter in _parameters)
            {
                var values = arguments[parameter.Names];
            }

            return null;
        }
    }
}
