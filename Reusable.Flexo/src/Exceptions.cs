using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public class InvalidExpressionException : Exception
    {
        public InvalidExpressionException(Type expectedExpressionType, Type actualExpressionType)
            : base($"Invalid expression type. Expected: {expectedExpressionType.ToPrettyString()}; Actual: {actualExpressionType.ToPrettyString()}.")
        { }
    }

    public class MissingInParameterException : Exception
    {
        public MissingInParameterException(IEnumerable<IParameterAttribute> parameters)
            : base($"One or more in parameters are missing: [{string.Join(", ", parameters.Select(x => x.Name))}]")
        { }
    }

    public class MissingOutParameterException : Exception
    {
        public MissingOutParameterException(IEnumerable<IParameterAttribute> parameters)
            : base($"One or more out parameters are missing: [{string.Join(", ", parameters.Select(x => x.Name))}]")
        { }
    }
}
