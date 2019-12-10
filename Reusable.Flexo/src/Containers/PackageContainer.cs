using Reusable.Data;

namespace Reusable.Flexo.Containers
{
    public delegate Package? GetPackageFunc(string packageId);

    public class PackageContainer : ReadOnlyContainer<string, Package>
    {
        private readonly GetPackageFunc _getPackage;

        public PackageContainer(GetPackageFunc getPackage) => _getPackage = getPackage;

        public override Maybe<Package> GetItem(string key) => (_getPackage(key), key);
    }
}