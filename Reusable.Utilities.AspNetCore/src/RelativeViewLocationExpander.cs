using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Reusable.Utilities.AspNetCore
{
    public class RelativeViewLocationExpander : IViewLocationExpander
    {
        private readonly string _prefix;

        public RelativeViewLocationExpander(string prefix)
        {
            _prefix = prefix;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values[nameof(RelativeViewLocationExpander)] = nameof(RelativeViewLocationExpander);
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return viewLocations.Select(viewLocation => $"/{_prefix}{viewLocation}");
        }
    }
}
