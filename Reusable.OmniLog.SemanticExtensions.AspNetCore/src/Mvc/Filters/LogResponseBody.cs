using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.Beaver;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Enables logging of the response body by the Semantic Logger Middleware.
    /// </summary>
    [UsedImplicitly]
    public class LogResponseBody : ActionFilterAttribute
    {
        private readonly IFeatureAgent _featureToggle;

        public LogResponseBody(IFeatureAgent featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _featureToggle.SetPolicy(Features.LogResponseBody, FeaturePolicy.AlwaysOn);
            _featureToggle.Telemetry(Features.LogResponseBody, true);
        }
    }
}