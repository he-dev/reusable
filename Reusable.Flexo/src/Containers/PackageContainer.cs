using Reusable.Data;

namespace Reusable.Flexo.Containers
{
    /// <summary>
    /// This delegate implements getting packages. 
    /// </summary>
    public delegate Package? GetPackageFunc(string packageId);

    /// <summary>
    /// This package container provides packages.
    /// </summary>
    public class PackageContainer : ReadOnlyContainer<string, Package>
    {
        private readonly GetPackageFunc _getPackage;

        public PackageContainer(GetPackageFunc getPackage) => _getPackage = getPackage;

        public override Maybe<Package> GetItem(string key) => Maybe.SingleRef(_getPackage(key), key);
    }
}