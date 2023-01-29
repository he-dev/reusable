using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reusable.Wiretap.Utilities.AspNetCore.Mvc.Filters;

/// <summary>
/// Enables logging of the response body by the Semantic Logger Middleware.
/// </summary>
[UsedImplicitly]
public class CanLogResponseBody : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items[nameof(CanLogResponseBody)] = true;
    }
}