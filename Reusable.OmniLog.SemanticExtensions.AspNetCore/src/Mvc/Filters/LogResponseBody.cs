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
        private readonly IFeatureController _featureToggle;

        public LogResponseBody(IFeatureController featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _featureToggle.Add(new Feature(Features.LogResponseBody, FeaturePolicy.AlwaysOn));
            //_featureToggle.AddOrUpdate(new Feature.Telemetry(Features.LogResponseBody, FeaturePolicy.AlwaysOn));
        }
    }
}