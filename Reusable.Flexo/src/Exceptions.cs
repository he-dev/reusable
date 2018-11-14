using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Flexo.Expressions;

namespace Reusable.Flexo
{
    public class InvalidExpressionException : Exception
    {
        public InvalidExpressionException(Type expectedExpressionType, Type actualExpressionType)
            : base($"Invalid expression type. Expected: {expectedExpressionType.Name}; Actual: {actualExpressionType.Name}.")
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
