using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Utilities.AspNetCore.ActionFilters;

public class ValidateModel : ActionFilterAttribute
{
    private readonly ILogger _logger;

    public ValidateModel(ILogger logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        // _logger.Log(
        //     Telemetry
        //         .Collect
        //         .Application()
        //         .Metadata(new { ModelErrors = context.ModelState.Values.Select(value => value.Errors.Select(error => error.Exception.Message)) }));

        context.Result = new BadRequestObjectResult(context.ModelState);
    }
}