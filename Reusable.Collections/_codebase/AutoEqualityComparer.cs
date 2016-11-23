using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class AutoEqualityComparer<TArg, TProjection> : EqualityComparer<TArg>
    {
        private readonly Func<TArg, TProjection> _projection;

        public AutoEqualityComparer(Func<TArg, TProjection> projection)
        {
            _projection = projection;
        }

        public override bool Equals(TArg left, TArg right)
        {
            if (left == null && right == null)
            {
                return true;
            }
            if (left == null)
            {
                return false;
            }
            if (right == null)
            {
                return false;
            }

            var valueLeft = _projection(left);
            var valueRight = _projection(right);

            return EqualityComparer<TProjection>.Default.Equals(valueLeft, valueRight);
        }

        public override int GetHashCode(TArg obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var objData = _projection(obj);

            return EqualityComparer<TProjection>.Default.GetHashCode(objData);
        }
    }
}
