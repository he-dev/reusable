using Reusable.Translucent.Data;

namespace Reusable.Translucent.Extensions;

public static class ResponseExtensions
{
    public static bool Exists(this Response response) => response.StatusCode == ResourceStatusCode.Success;
}