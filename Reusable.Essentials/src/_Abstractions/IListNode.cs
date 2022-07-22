namespace Reusable.Essentials;

public interface IListNode<T> where T : IListNode<T>
{
    T? Prev { get; set; }

    T? Next { get; set; }
}

public abstract class ListNode<T> : IListNode<T> where T : IListNode<T>
{
    public virtual T? Prev { get; set; }

    public virtual T? Next { get; set; }
}