using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gems.Cryptography.Extensions;
using Gems.Emerald.Servers.Vault.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gems.Emerald.Servers.Vault.ActionFilters
{
    public class RequestFingerprintActionFilter : IActionFilter
    {
        private readonly string _headerPefix;
        private readonly Func<HttpContext, string, byte[]> _getRequestFingerprint;

        public RequestFingerprintActionFilter(string headerPefix, Func<HttpContext, string, byte[]> getRequestFingerprint)
        {
            _headerPefix = headerPefix;
            _getRequestFingerprint = getRequestFingerprint;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["RequestFingerprint"] = _getRequestFingerprint(context.HttpContext, _headerPefix).ToHexString();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // there's nothing to do
        }
    }
}
