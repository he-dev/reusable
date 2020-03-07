using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Collections.Generic
{
    public interface INode<T> where T : class, INode<T>
    {
        T? Prev { get; set; }

        T? Next { get; set; }
    }
}