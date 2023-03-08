﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Reusable.Extensions;

public static class StackExtensions
{
    public static void Add<T>(this Stack<T> stack, T? item)
    {
        if (item is { })
        {
            stack.Push(item);
        }
    }
    
    public static bool TryPush<T>(this Stack<T> stack, T? item)
    {
        if (item is { })
        {
            stack.Push(item);
            return true;
        }

        return false;
    }

    public static bool TryPeek<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T item)
    {
        if (stack.Count > 0)
        {
            item = stack.Peek();
            return true;
        }

        item = default;
        return false;
    }

    public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            stack.TryPush(item);
        }
    }

    public static IEnumerable<T> Consume<T>(this Stack<T> queue)
    {
        while (queue.Count > 0)
        {
            yield return queue.Pop();
        }
    }
}