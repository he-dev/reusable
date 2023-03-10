using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reusable.Wiretap.AspNetCore.Mvc.Filters;

public class LogResponseBody : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Request.EnableBuffering();
        context.HttpContext.Items[nameof(LogResponseBody)] = true;
    }
    
    public static bool Enabled(HttpContext context) => context.Items.TryGetValue(nameof(LogResponseBody), out var value) && value is true;
}