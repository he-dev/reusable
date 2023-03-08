using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.Collections;

internal class AutoEqualityPropertyContext
{
    public PropertyInfo Property { get; set; } = default!;

    public AutoEqualityPropertyAttribute Attribute { get; set; } = default!;

    public Expression LeftParameter { get; set; } = default!;

    public Expression RightParameter { get; set; } = default!;
}