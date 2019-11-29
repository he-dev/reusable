namespace Reusable.Flexo.Abstractions
{
    public interface IContainer<T>
    {
        Maybe<T> GetItem(string key);
    }
}