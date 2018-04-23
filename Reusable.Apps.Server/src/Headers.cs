using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable;

namespace Gems.Emerald.Servers.Vault
{
    [PublicAPI]
    public static class Headers
    {
        public static readonly string Prefix = "X-Vault-";

        public static readonly string ApiVersion = $"{Prefix}{nameof(ApiVersion)}";

        public static readonly string Machine = $"{Prefix}{nameof(Machine)}";
        public static readonly string Environment = $"{Prefix}{nameof(Environment)}";
        public static readonly string Product = $"{Prefix}{nameof(Product)}";
        public static readonly string ProductVersion = $"{Prefix}{nameof(ProductVersion)}";
        public static readonly string SettingVersion = $"{Prefix}{nameof(SettingVersion)}";
        public static readonly string User = $"{Prefix}{nameof(User)}";
        public static readonly string Role = $"{Prefix}{nameof(Role)}";

        public static readonly string Transaction = $"{Prefix}{nameof(Transaction)}";

        public static IImmutableList<SoftString> ClientHandle = ImmutableList.CreateRange(new SoftString[]
        {
            Machine,
            Environment,
            Product,
            ProductVersion,
            SettingVersion,
            User,
            Role,
        });
    }    
}
