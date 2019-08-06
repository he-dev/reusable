using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Enables logging of the response body by the Semantic Logger Middleware.
    /// </summary>
    [UsedImplicitly]
    public class LogResponseBody : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.EnableResponseBodyLogging();
        }
    }
}
