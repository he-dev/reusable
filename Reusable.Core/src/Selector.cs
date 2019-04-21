namespace Reusable
{
    public interface ISelector<T> { }

    public static class Select
    {
        public static ISelector<T> From<T>() => default;
    }
}