using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Reusable.Toggle;
using Reusable.Toggle.Mechanics;

namespace Reusable.Wiretap.Utilities.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Enables logging of the response body by the Semantic Logger Middleware.
    /// </summary>
    [UsedImplicitly]
    public class LogResponseBody : ActionFilterAttribute
    {
        private readonly IFeatureService _features;

        public LogResponseBody(IFeatureService features)
        {
            _features = features;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _features.TryAdd(new FeatureMechanics(WiretapMiddleware.Features.LogResponseBody, FeaturePolicy.AlwaysEnabled));
        }
    }
}