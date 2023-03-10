namespace Reusable.Wiretap.AspNetCore.Abstractions;

public interface ITakeSnapshot<in T>
{
    object Invoke(T entity);
}