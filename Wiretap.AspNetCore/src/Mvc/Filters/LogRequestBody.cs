using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reusable.Wiretap.AspNetCore.Mvc.Filters;

/// <summary>
/// Enables logging of the response body by the Semantic Logger Middleware.
/// </summary>
[UsedImplicitly]
public class LogRequestBody : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Request.EnableBuffering();
        context.HttpContext.Items[nameof(LogRequestBody)] = true;
    }

    public static bool Enabled(HttpContext context) => context.Items.TryGetValue(nameof(LogRequestBody), out var value) && value is true;
}