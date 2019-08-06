namespace Reusable.OmniLog.Abstractions
{
    public interface ILinkedListNode<T>
    {
        T Previous { get; }

        T Next { get; }

        T InsertNext(T next);

        /// <summary>
        /// Removes this node from the chain an returns the Previous item or Next if Previous is null.
        /// </summary>
        /// <returns></returns>
        T Remove();
    }
}