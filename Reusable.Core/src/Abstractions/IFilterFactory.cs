using System;

namespace Reusable
{
    public interface IFilterFactory<in TElement, in TCondition>
    {
        Func<TElement, bool> Create(TCondition condition);
    }
}