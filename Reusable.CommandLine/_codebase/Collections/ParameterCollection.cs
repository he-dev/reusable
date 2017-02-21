using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Input;
using System.Reflection;
using Reusable.Shelly;
using Reusable.Shelly.Validators;
using System.Collections.Immutable;

namespace Reusable.Shelly.Collections
{
    internal class ParameterCollection : IEnumerable<Data.ParameterInfo>
    {
        private readonly IImmutableList<Data.ParameterInfo> _parameters;

        public ParameterCollection(Type parameterType)
        {
            if (!parameterType.HasDefaultConstructor()) throw new ArgumentException($"'{nameof(parameterType)}' '{parameterType}' must have a default constructor.");

            ParameterType = parameterType;
            _parameters = ParameterReflector.GetParameters(parameterType).ToImmutableList();
            ParameterValidator.ValidateParameterNamesUniqueness(_parameters);
        }

        public Type ParameterType { get; }

        #region IEnumerable

        public IEnumerator<Data.ParameterInfo> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
