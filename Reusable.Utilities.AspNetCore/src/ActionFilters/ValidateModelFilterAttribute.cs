using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Utilities.AspNetCore.ActionFilters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public ValidateModelAttribute(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidateModelAttribute>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                return;
            }

            _logger.Log(
                Abstraction.Layer.Network().Data().Object(new
                {
                    ModelState = context.ModelState.Values.Select(value => value.Errors.Select(error => error.Exception.Message))
                }),
                log => log.Level(LogLevel.Error)
            );

            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
