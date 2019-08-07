using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Data
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class TreeNode<T> : List<TreeNode<T>>
    {
        public TreeNode(T value, IEnumerable<TreeNode<T>> children) : base(children)
        {
            Value = value;
        }

        public TreeNode(T value) : this(value, Enumerable.Empty<TreeNode<T>>()) { }

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

    public static class TreeNodeExtensions
    {
        public static (TreeNode<T> Parent, TreeNode<T> Child) Add<T>(this TreeNode<T> parent, T value)
        {
            var child = new TreeNode<T>(value);
            parent.Add(child);
            return (parent, child);
        }

        public static IEnumerable<TView> Views<T, TView>(this TreeNode<T> root, RenderTreeNodeValueCallback<T, TView> renderTreeNodeValue) where TView : ITreeNodeView
        {
            return Views(root, 0, renderTreeNodeValue);
        }

        private static IEnumerable<TView> Views<T, TView>(TreeNode<T> root, int depth, RenderTreeNodeValueCallback<T, TView> renderTreeNodeValue) where TView : ITreeNodeView
        {
            yield return renderTreeNodeValue(root, depth);

            var views =
                from node in root
                from view in Views(node, depth + 1, renderTreeNodeValue)
                select view;

            foreach (var view in views)
            {
                yield return view;
            }
        }

        public static string Render(this IEnumerable<TreeNodePlainView> views, int indentWidth = 3)
        {
            return
                views
                    .Select(nv => nv.Text.IndentLines(indentWidth * nv.Depth))
                    .Join(Environment.NewLine);
        }
    }

    public interface ITreeNodeView
    {
        int Depth { get; }
    }

    public class TreeNodePlainView : ITreeNodeView
    {
        public string Text { get; set; }

        public int Depth { get; set; }
    }

    public delegate TView RenderTreeNodeValueCallback<in T, out TView>(T value, int depth) where TView : ITreeNodeView;

    [PublicAPI]
    public interface ITreeRenderer<out TResult, in TView> where TView : ITreeNodeView
    {
        TResult Render<TValue>(TreeNode<TValue> root, RenderTreeNodeValueCallback<TValue, TView> renderTreeNodeValue);
    }

    public class PlainTreeRenderer : ITreeRenderer<string, TreeNodePlainView>
    {
        public int IndentWidth { get; set; } = 3;

        public string Render<TValue>(TreeNode<TValue> root, RenderTreeNodeValueCallback<TValue, TreeNodePlainView> renderTreeNodeValue)
        {
            return 
                root
                    .Views(renderTreeNodeValue)
                    .Render(IndentWidth);
        }
    }
}