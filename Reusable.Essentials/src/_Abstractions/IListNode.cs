namespace Reusable.Essentials;

public interface IListNode<T> where T : IListNode<T>
{
    T? Prev { get; set; }

    T? Next { get; set; }
}