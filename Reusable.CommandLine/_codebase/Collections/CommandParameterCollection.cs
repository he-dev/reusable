using Reusable.Shelly.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Input;
using System.Reflection;

namespace Reusable.Shelly.Collections
{
    public class CommandParameterCollection : IEnumerable<CommandParameterInfo>
    {
        private readonly IDictionary<StringSet, CommandParameterInfo> _parameters;

        public CommandParameterCollection(Type parameterType, IEnumerable<CommandParameterInfo> parameters)
        {
            if (!parameterType.HasDefaultConstructor()) throw new ArgumentException($"'{nameof(parameterType)}' '{parameterType}' must have a default constructor.");

            ParameterType = parameterType;
            _parameters = parameters.ToDictionary<CommandParameterInfo, StringSet, CommandParameterInfo>(keySelector: x => x.Names, elementSelector: x => x, comparer: new HashSetOverlapsComparer<string>());
        }

        public CommandParameterCollection(Type parameterType)
            : this(
                  parameterType,
                  parameterType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                    .Select(p => new CommandParameterInfo(p)))
        { }

        public CommandParameterInfo this[StringSet nameSet] => _parameters.TryGetValue(nameSet, out CommandParameterInfo parameter) ? parameter : null;

        public CommandParameterInfo this[string name] => this[StringSet.CreateCI(name)];

        public Type ParameterType { get; }

        public void Add(CommandParameterInfo parameter) => _parameters.Add(parameter.Names, parameter);

        #region IEnumerable

        public IEnumerator<CommandParameterInfo> GetEnumerator() => _parameters.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
