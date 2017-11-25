using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable
{
    public static class AppDomainPaths
    {
        /// <summary>
        /// Gets the name of the directory containing the configuration file for an application domain.
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static IEnumerable<string> ConfigurationFile([CanBeNull] AppDomain appDomain = null)
        {
            yield return Path.GetDirectoryName((appDomain ?? AppDomain.CurrentDomain).SetupInformation.ConfigurationFile);
        }

        /// <summary>
        /// Gets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static IEnumerable<string> BaseDirectory([CanBeNull] AppDomain appDomain = null)
        {
            yield return (appDomain ?? AppDomain.CurrentDomain).BaseDirectory;
        }

        /// <summary>
        /// Gets the name of the application base directory.
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static IEnumerable<string> ApplicationBase([CanBeNull] AppDomain appDomain = null)
        {
            yield return (appDomain ?? AppDomain.CurrentDomain).SetupInformation.ApplicationBase;
        }

        /// <summary>
        /// Gets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static IEnumerable<string> PrivateBin([CanBeNull] AppDomain appDomain = null)
        {
            return
                (appDomain ?? AppDomain.CurrentDomain)
                    .SetupInformation
                    .PrivateBinPath
                    ?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                ?? Enumerable.Empty<string>();
        }
    }
}
