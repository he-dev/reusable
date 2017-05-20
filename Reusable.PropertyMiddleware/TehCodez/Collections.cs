using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Reusable.ExpressProperty
{
    public class ExpressionList<T> : List<Expression<Func<T, object>>> { }

    public class ExpressionDictionary<T> : Dictionary<Expression<Func<T, object>>, object> { }
}
