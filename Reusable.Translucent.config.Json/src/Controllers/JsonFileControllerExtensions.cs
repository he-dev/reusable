using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class JsonFileControllerExtensions
    {
        public static IResourceControllerBuilder AddJsonFile(this IResourceControllerBuilder builder, string basePath, string fileName)
        {
            return builder.AddController(new JsonFileController(basePath, fileName));
        }
    }
}