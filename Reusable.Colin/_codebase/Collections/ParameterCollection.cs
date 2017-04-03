using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Input;
using System.Reflection;
using Reusable.Colin;
using System.Collections.Immutable;
using Reusable.Colin.Validators;
using Reusable.Extensions;

namespace Reusable.Colin.Collections
{
   internal class ParameterCollection : IEnumerable<Data.ParameterInfo>
    {
        private readonly IImmutableList<Data.ParameterInfo> _parameters;

        private ParameterCollection() => _parameters = ImmutableList<Data.ParameterInfo>.Empty;

        private ParameterCollection(Type parameterType)
        {
            if (!parameterType.HasDefaultConstructor()) throw new ArgumentException($"'{nameof(parameterType)}' '{parameterType}' must have a default constructor.");

            ParameterType = parameterType;
            _parameters = ParameterReflector.GetParameters(parameterType).ToImmutableList();
            ParameterValidator.ValidateParameterNamesUniqueness(_parameters);
        }

        public static ParameterCollection Empty => new ParameterCollection();

        public Type ParameterType { get; }

        public static ParameterCollection Create(Type parameterType) => parameterType == null ? Empty : new ParameterCollection(parameterType);

        #region IEnumerable

        public IEnumerator<Data.ParameterInfo> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
