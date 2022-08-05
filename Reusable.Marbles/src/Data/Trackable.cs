using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles.Data;

public class Trackable<T> : Stack<T>, IDisposable
{
    public T Value
    {
        get => Peek();
        set => Push(value);
    }

    public bool HasValue => Count > 0;
    
    public void Dispose()
    {
        foreach (var item in this.Consume().OfType<IDisposable>())
        {
            item.Dispose();
        }
    }

    public static implicit operator bool(Trackable<T> trackable) => trackable.HasValue;
}