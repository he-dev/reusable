namespace Reusable.Marbles;

public interface IIdentifiable<out T>
{
    T Name { get; }
}