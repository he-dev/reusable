using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.CommandLine.Data
{
    public class ParameterMetadata : IEnumerable<ArgumentMetadata>
    {
        private readonly IEnumerable<ArgumentMetadata> _parameters;

        public ParameterMetadata([CanBeNull] Type parameterType, [NotNull, ItemNotNull] IEnumerable<ArgumentMetadata> parameters)
        {
            ParameterType = parameterType;
            _parameters = parameters;
        }

        [CanBeNull]
        public Type ParameterType { get; }

        public IEnumerator<ArgumentMetadata> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}