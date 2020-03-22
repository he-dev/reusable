using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.Beaver;

namespace Reusable.OmniLog.Utilities.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Enables logging of the response body by the Semantic Logger Middleware.
    /// </summary>
    [UsedImplicitly]
    public class LogResponseBody : ActionFilterAttribute
    {
        private readonly IFeatureController _featureToggle;

        public LogResponseBody(IFeatureController featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _featureToggle.Add(SemanticLogger.Features.LogResponseBody, FeaturePolicy.AlwaysOn);
        }
    }
}