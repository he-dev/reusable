using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.OmniLog;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Utilities.AspNetCore.ActionFilters
{
    public class ValidateModel : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public ValidateModel(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidateModel>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                return;
            }

            _logger.Log(
                Telemetry
                    .Collect
                    .Application()
                    .Metadata("ModelErrors", context.ModelState.Values.Select(value => value.Errors.Select(error => error.Exception.Message)))
                    .Error());

            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}