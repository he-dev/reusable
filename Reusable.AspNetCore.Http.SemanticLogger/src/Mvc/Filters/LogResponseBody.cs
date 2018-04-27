using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.AspNetCore.Http;

namespace Mailr.Utilities.Mvc.Filters
{
    public class LogResponseBody : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.EnableResponseBodyLogging();
        }
    }
}
