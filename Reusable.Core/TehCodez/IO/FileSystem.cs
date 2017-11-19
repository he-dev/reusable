using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IO
{
    public interface IFileSystem
    {
        [NotNull, ItemNotNull]
        IEnumerable<string> EnumerateDirectories([NotNull] string path, [CanBeNull] Func<string, bool> exclude = null, bool deep = true);

        [NotNull, ItemNotNull]
        IEnumerable<string> EnumerateFiles(string path);
    }
    
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> EnumerateDirectories(string path, Func<string, bool> exclude, bool deep = true)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!Path.IsPathRooted(path)) throw new ArgumentException(paramName: nameof(path), message: $"{nameof(path)} must be rooted. Example: 'C:\\some\\path'");

            exclude = exclude ?? PathFilterFactory.None;

            var directories = new Stack<string>
            {
                path,
            };

            yield return path;

            while (directories.Any())
            {
                foreach (var directory in Directory.EnumerateDirectories(directories.Pop()).Skip(exclude))
                {
                    yield return directory;

                    if (deep)
                    {
                        directories.Push(directory);
                    }
                }
            }
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return
                from file in Directory.EnumerateFiles(path)
                select file;
        }
    }
}