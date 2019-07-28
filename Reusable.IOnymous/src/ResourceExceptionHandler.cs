using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.IOnymous
{
    public class ResourceExceptionHandler : IResource
    {
        private readonly IResource _resource;

        public ResourceExceptionHandler([NotNull] IResource resource) => _resource = resource ?? throw new ArgumentNullException(nameof(resource));

        public IImmutableContainer Properties => _resource.Properties;

        public UriString Uri => _resource.Uri;

        public bool Exists => _resource.Exists;

        public MimeType Format => _resource.Format;

        public async Task CopyToAsync(Stream stream)
        {
            if (!Exists)
            {
                throw new InvalidOperationException($"Cannot copy resource '{Uri}' because it does not exist.");
            }

            try
            {
                await _resource.CopyToAsync(stream);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("CopyTo", $"An error occured while trying to copy the resource '{Uri}'. See the inner exception for details.", inner);
            }
        }

        public bool Equals(IResource other) => _resource.Equals(other);

        public bool Equals(string other) => _resource.Equals(other);

        public void Dispose() => _resource.Dispose();
    }
}