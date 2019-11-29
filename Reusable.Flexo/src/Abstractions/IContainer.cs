namespace Reusable.Flexo.Abstractions
{
    public interface IContainer<T>
    {
        Option<T> GetItem(string key);
    }
}