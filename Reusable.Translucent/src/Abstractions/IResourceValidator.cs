using Reusable.Translucent.Middleware;

namespace Reusable.Translucent.Abstractions
{
    public interface IResourceValidator
    {
        void Validate(ResourceContext context);
    }
}