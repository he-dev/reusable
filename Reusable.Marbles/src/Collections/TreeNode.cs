using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Marbles.Diagnostics;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles.Collections;

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
        b.DisplaySingle(x => x.Value);
        b.DisplaySingle(x => x.Count);
    });

    public static TreeNode<T> Empty => new(default);

    public T Value { get; set; }

    public new TreeNode<T> Add(TreeNode<T> treeNode)
    {
        base.Add(treeNode);
        return treeNode;
    }

    public static implicit operator T(TreeNode<T> treeNode) => treeNode.Value;

    public static implicit operator TreeNode<T>(T value) => new(value);
}

public static class TreeNode
{
    public static TreeNode<T> Create<T>(T value) => new(value);
}

public static class TreeNodeExtensions
{
    public static (TreeNode<T> Parent, TreeNode<T> Child) Add<T>(this TreeNode<T> parent, T value)
    {
        var child = new TreeNode<T>(value);
        parent.Add(child);
        return (parent, child);
    }

    public static IEnumerable<TView> Views<T, TView>(this TreeNode<T> root, RenderTreeNodeValueDelegate<T, TView> renderTreeNodeValue) where TView : INodeView
    {
        return Views(root, 0, renderTreeNodeValue);
    }

    private static IEnumerable<TView> Views<T, TView>(TreeNode<T> root, int depth, RenderTreeNodeValueDelegate<T, TView> renderTreeNodeValue) where TView : INodeView
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

public interface INodeView
{
    int Depth { get; }
}

public class TreeNodePlainView : INodeView
{
    public string Text { get; set; }

    public int Depth { get; set; }
}

public delegate TView RenderTreeNodeValueDelegate<in T, out TView>(T value, int depth) where TView : INodeView;

[PublicAPI]
public interface ITreeRenderer<out TResult, in TView> where TView : INodeView
{
    TResult Render<TValue>(TreeNode<TValue> root, RenderTreeNodeValueDelegate<TValue, TView> renderTreeNodeValue);
}

public class PlainTreeRenderer : ITreeRenderer<string, TreeNodePlainView>
{
    public static ITreeRenderer<string, TreeNodePlainView> Default { get; } = new PlainTreeRenderer();
        
    public int IndentWidth { get; set; } = 3;

    public string Render<TValue>(TreeNode<TValue> root, RenderTreeNodeValueDelegate<TValue, TreeNodePlainView> renderTreeNodeValue)
    {
        return 
            root
                .Views(renderTreeNodeValue)
                .Render(IndentWidth);
    }
}