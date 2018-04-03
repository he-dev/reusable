using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class ProjectionComparer<TArg, TProjection> : EqualityComparer<TArg>
    {
        private readonly Func<TArg, TProjection> _projectFunc;

        public ProjectionComparer(Func<TArg, TProjection> projectFunc)
        {
            _projectFunc = projectFunc ?? throw new ArgumentNullException(nameof(projectFunc));
        }

        public override bool Equals(TArg left, TArg right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return false;
            return
                ReferenceEquals(left, right) ||
                EqualityComparer<TProjection>.Default.Equals(
                    _projectFunc(left), 
                    _projectFunc(right)
                );
        }

        public override int GetHashCode(TArg obj)
        {
            return EqualityComparer<TProjection>.Default.GetHashCode(_projectFunc(obj));
        }
    }
    

    public static class ProjectionComparer<TArg>
    {
        public static IEqualityComparer<TArg> Create<TProjection>(Func<TArg, TProjection> projection)
        {
            return new ProjectionComparer<TArg, TProjection>(projection);
        }
    }
}
