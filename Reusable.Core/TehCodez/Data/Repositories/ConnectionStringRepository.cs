using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Data.Repositories
{
    public interface IConnectionStringRepository
    {
        [CanBeNull]
        [ContractAnnotation("nameOrConnectionString: null => halt")]
        string GetConnectionString([NotNull] string nameOrConnectionString);
    }

    public class ConnectionStringRepository : IConnectionStringRepository
    {
        [NotNull]
        public static readonly IConnectionStringRepository Default = new ConnectionStringRepository();

        public string GetConnectionString(string nameOrConnectionString)
        {
            if (string.IsNullOrEmpty(nameOrConnectionString)) throw new ArgumentNullException(nameof(nameOrConnectionString));
            var match = Regex.Match(nameOrConnectionString, @"\Aname=(?<name>.+)", RegexOptions.IgnoreCase);
            return match.Success ? ConfigurationManager.ConnectionStrings[match.Groups["name"].Value]?.ConnectionString : nameOrConnectionString;
        }
    }
}
