using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reusable.AspNetCore.Http.Mvc.Filters
{
    [UsedImplicitly]
    public class LogResponseBody : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.EnableResponseBodyLogging();
        }
    }
}
