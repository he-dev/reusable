using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.Beaver;
using Reusable.Beaver.Policies;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Enables logging of the response body by the Semantic Logger Middleware.
    /// </summary>
    [UsedImplicitly]
    public class LogResponseBody : ActionFilterAttribute
    {
        private readonly IFeatureToggle _featureToggle;

        public LogResponseBody(IFeatureToggle featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _featureToggle.SetOrUpdate(nameof(LogResponseBody), FeaturePolicy.AlwaysOn);
        }
    }
}
