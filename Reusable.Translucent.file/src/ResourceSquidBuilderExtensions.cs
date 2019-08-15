using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidBuilderExtensions
    {
        public static ResourceSquidBuilder UseEmbeddedFiles<T>(this ResourceSquidBuilder builder, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                builder.UseController(new EmbeddedFileController<T>(basePath));
            }

            return builder;
        }
        
        public static ResourceSquidBuilder UsePhysicalFiles<T>(this ResourceSquidBuilder builder, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                builder.UseController(new PhysicalFileController(basePath));
            }

            return builder;
        }
        
        public static ResourceSquidBuilder UseInMemoryFiles<T>(this ResourceSquidBuilder builder, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                builder.UseController(new EmbeddedFileController<T>(basePath));
            }

            return builder;
        }
    }
}