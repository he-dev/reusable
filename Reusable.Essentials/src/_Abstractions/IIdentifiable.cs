namespace Reusable.Essentials;

public interface IIdentifiable<out T>
{
    T Name { get; }
}