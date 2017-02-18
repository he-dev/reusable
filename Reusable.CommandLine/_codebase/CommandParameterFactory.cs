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
    internal class CommandParameterFactory
    {
        public static object CreateCommandParameter(Type parameterType, ArgumentCollection arguments, TypeConverter converter)
        {
            var parameter = Activator.CreateInstance(parameterType);
            var parameterProperties = parameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttribute<ParameterAttribute>() != null);

            foreach (var property in parameterProperties)
            {
                //var names = property.GetCustomAttribute<ParameterAttribute>
            }

            return null;
        }
    }
}
