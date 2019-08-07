using System;
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
            b.DisplayScalar(x => x.Value);
            b.DisplayScalar(x => x.Count);
        });

        [NotNull]
        public static TreeNode<T> Empty => new TreeNode<T>(default);

        [NotNull]
        public T Value { get; set; }

        public new TreeNode<T> Add(TreeNode<T> node)
        {
            base.Add(node);
            return node;
        }

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

    public delegate string RenderValueCallback<in T>(T value, int depth);

    [PublicAPI]
    public interface ITreeRenderer<out TResult>
    {
        TResult Render<TValue>(TreeNode<TValue> root, RenderValueCallback<TValue> renderValue);
    }

    public abstract class TreeRenderer<TResult> : ITreeRenderer<TResult>
    {
        public abstract TResult Render<TValue>(TreeNode<TValue> root, RenderValueCallback<TValue> renderValue);
    }

    public class PlainTextTreeRenderer : ITreeRenderer<string>
    {
        public int IndentWidth { get; set; } = 3;

        public string Render<TValue>(TreeNode<TValue> root, RenderValueCallback<TValue> renderValue)
        {
            var nodeViews = Render(root, 0, renderValue);
            var indentedNodeViews = nodeViews.Select(nv => nv.Value.IndentLines(IndentWidth * nv.Depth));
            return string.Join(Environment.NewLine, indentedNodeViews);
        }

        private static IEnumerable<(string Value, int Depth)> Render<T>(TreeNode<T> root, int depth, RenderValueCallback<T> renderValue)
        {
            yield return (renderValue(root, depth), depth);

            var views =
                from node in root
                from view in Render(node, depth + 1, renderValue)
                select view;

            foreach (var view in views)
            {
                yield return view;
            }
        }
    }
}