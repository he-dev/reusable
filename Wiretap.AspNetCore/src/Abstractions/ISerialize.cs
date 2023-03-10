using System.Threading.Tasks;

namespace Reusable.Wiretap.AspNetCore.Abstractions;

public interface ISerialize<in T>
{
    Task<string?> Invoke(T entity);
}