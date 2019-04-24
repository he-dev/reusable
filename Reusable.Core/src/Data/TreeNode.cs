using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Data
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class TreeNode<T> : List<TreeNode<T>>
    {
        public TreeNode(T value, IEnumerable<TreeNode<T>> children)
            : base(children)
        {
            Value = value;
        }

        public TreeNode(T value)
            : this(value, Enumerable.Empty<TreeNode<T>>()) { }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayValue(x => x.Value);
            b.DisplayValue(x => x.Count);
        });

        [NotNull]
        public static TreeNode<T> Empty => new TreeNode<T>(default);

        [CanBeNull]
        public T Value { get; set; }

        public static implicit operator T(TreeNode<T> node) => node.Value;

        public static implicit operator TreeNode<T>(T value) => new TreeNode<T>(value);
    }

    public static class TreeNode
    {
        public static TreeNode<T> Create<T>(T value) => new TreeNode<T>(value);
    }

//    public class TreeNode : TreeNode<object>
//    {
//        public TreeNode(object obj) : base(obj) { }
//
//        public new static TreeNode Empty => new TreeNode(default);
//
//        public override TreeNode<object> Add(object obj)
//        {
//            return base.Add(obj is TreeNode<object> tn ? tn : new TreeNode<object>(obj));
//        }        
//    }

    public static class TreeNodeExtensions
    {
        public static (TreeNode<T> Parent, TreeNode<T> Child) Add<T>(this TreeNode<T> parent, T value)
        {
            var child = new TreeNode<T>(value);
            parent.Add(child);
            return (parent, child);
        }
    }
}