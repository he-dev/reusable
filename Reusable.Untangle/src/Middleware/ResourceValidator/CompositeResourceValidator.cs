using System.Collections.Generic;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Middleware.ResourceValidator
{
    public class CompositeResourceValidator : List<IResourceValidator>, IResourceValidator
    {
        public void Validate(ResourceContext context)
        {
            foreach (var validator in this)
            {
                validator.Validate(context);
            }
        }
    }
}