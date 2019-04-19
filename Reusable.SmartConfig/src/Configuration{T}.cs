using JetBrains.Annotations;
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    // ReSharper disable once UnusedTypeParameter - this is by design and is for chaining extensions
    public interface IConfiguration<T> : IConfiguration { }

    public class Configuration<T> : Configuration, IConfiguration<T>
    {
        public Configuration([NotNull] IResourceProvider settingProvider) : base(settingProvider) { }
    }
}