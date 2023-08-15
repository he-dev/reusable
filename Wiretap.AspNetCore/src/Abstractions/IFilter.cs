using Microsoft.AspNetCore.Http;

namespace Reusable.Wiretap.AspNetCore.Abstractions;

public interface IFilter<T>
{
    bool Matches(HttpContext context);
}