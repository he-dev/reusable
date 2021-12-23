namespace Reusable.Essentials;

public interface IListNode<T> where T : class, IListNode<T>
{
    T? Prev { get; set; }

    T? Next { get; set; }
}