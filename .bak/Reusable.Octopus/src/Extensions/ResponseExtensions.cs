using Reusable.Octopus.Data;

namespace Reusable.Octopus.Extensions;

public static class ResponseExtensions
{
    public static bool Success(this Response response) => response.StatusCode == ResourceStatusCode.Success;
    
    public static bool NotFound(this Response response) => response.StatusCode == ResourceStatusCode.NotFound;
}