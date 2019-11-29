using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Containers
{
    public delegate Package? GetPackageFunc(string packageId);

    public class PackageContainer : IContainer<Package>
    {
        private readonly GetPackageFunc _getPackage;

        public PackageContainer(GetPackageFunc getPackage) => _getPackage = getPackage;

        public Maybe<Package> GetItem(string key) => (_getPackage(key), key);
    }
}