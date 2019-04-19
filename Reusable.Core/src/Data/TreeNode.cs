using System.Collections.Generic;
using System.Linq;

namespace Reusable.Data
{
    public class TreeNode<T> : List<TreeNode<T>>
    {
        public TreeNode(T value, IEnumerable<TreeNode<T>> children)
            : base(children)
        {
            Value = value;
        }

        public TreeNode(T value)
            : this(value, Enumerable.Empty<TreeNode<T>>())
        { }

        public T Value { get; set; }

        public static implicit operator T(TreeNode<T> node) => node.Value;

        public static implicit operator TreeNode<T>(T value) => new TreeNode<T>(value);
    }
}