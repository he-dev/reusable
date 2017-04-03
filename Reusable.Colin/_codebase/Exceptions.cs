using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reusable.Colin
{
    public class ParameterNotFoundException : Exception
    {
        
    }

    public class ArgumentMappingException : Exception
    {
        public ArgumentMappingException(Exception innerException) : base(null, innerException) { }

        public string ArgumentName { get; set; }
        public string ArgumentType { get; set; }
        public string Value { get; set; }
    }

    public class CommnadNotFoundException : Exception
    {
        public CommnadNotFoundException(string message) : base(message) { }
    }

    public class DuplicateCommandNameException : Exception
    {
        public DuplicateCommandNameException(IImmutableSet<string> duplicateNames)
        {
            DuplicateNames = duplicateNames.ToImmutableList();
        }

        public ImmutableList<string> DuplicateNames { get; private set; }

        public override string Message => $"Duplicate command names [{string.Join(", ", DuplicateNames)}]' found.";
    }

    public class DuplicateParameterNameException : Exception
    {
        public DuplicateParameterNameException(Type parameterType, IEnumerable<string> duplicateNames)
        {
            ParameterType = parameterType;
            DuplicateNames = duplicateNames.ToImmutableList();
        }

        public Type ParameterType { get; }

        public ImmutableList<string> DuplicateNames { get; private set; }

        public override string Message => $"Duplicate names [{string.Join(", ", DuplicateNames)}]' found in {ParameterType.Name}'.";
    }
}
