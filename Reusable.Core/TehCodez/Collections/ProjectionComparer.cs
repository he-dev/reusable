using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class ProjectionComparer<TArg, TProjection> : EqualityComparer<TArg>
    {
        private readonly Func<TArg, TProjection> _project;

        public ProjectionComparer(Func<TArg, TProjection> projection)
        {
            _project = projection ?? throw new ArgumentNullException(nameof(projection));
        }

        public override bool Equals(TArg left, TArg right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                EqualityComparer<TProjection>.Default.Equals(
                    _project(left), 
                    _project(right)
                );
        }

        public override int GetHashCode(TArg obj)
        {
            return ReferenceEquals(obj, null) ? 0 : EqualityComparer<TProjection>.Default.GetHashCode(_project(obj));
        }
    }
    

    public class ProjectionComparer<TArg>
    {
        public static IEqualityComparer<TArg> Create<TProjection>(Func<TArg, TProjection> projection)
        {
            return new ProjectionComparer<TArg, TProjection>(projection);
        }
    }

}
