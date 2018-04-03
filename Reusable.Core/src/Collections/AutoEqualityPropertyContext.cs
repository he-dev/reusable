using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.Collections
{
    internal class AutoEqualityPropertyContext
    {
        public PropertyInfo Property { get; set; }

        public AutoEqualityPropertyAttribute Attribute { get; set; }

        public Expression LeftParameter { get; set; }

        public Expression RightParameter { get; set; }
    }
}