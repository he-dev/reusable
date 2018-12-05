using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;

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
            
            _logger.Log(Abstraction.Layer.Network().Meta(new
            {
                ModelErrors = context.ModelState.Values.Select(value => value.Errors.Select(error => error.Exception.Message))
            })
            .Error());

            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
