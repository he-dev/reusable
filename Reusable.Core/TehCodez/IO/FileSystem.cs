using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using System.Linq.Custom;

namespace Reusable.IO
{
    public interface IFileSystem
    {
        [NotNull, ItemNotNull]
        IEnumerable<string> EnumerateDirectories([NotNull] string path, [CanBeNull] Func<string, bool> exclude = null, bool deep = true);

        [NotNull, ItemNotNull]
        IEnumerable<string> EnumerateFiles(string path);

        bool Exists([NotNull] string path);

        [NotNull]
        string ReadAllText([NotNull] string fileName);

        string FindDirectory(string directoryName, IEnumerable<string> lookupPaths);

        string FindFile(string fileName, IEnumerable<string> lookupPaths);
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

        public bool Exists(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return File.Exists(path);
        }

        public string ReadAllText(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            return File.ReadAllText(fileName);
        }

        public string FindDirectory(string directoryName, IEnumerable<string> lookupPaths)
        {
            if (string.IsNullOrEmpty(directoryName)) throw new ArgumentNullException(nameof(directoryName));

            if (Path.IsPathRooted(directoryName)) { return directoryName; }

            return
                lookupPaths
                    .Select(currentDirectoryName => Path.Combine(currentDirectoryName, directoryName))
                    .FirstOrDefault(Directory.Exists);
        }

        public string FindFile(string fileName, IEnumerable<string> lookupPaths)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            if (Path.IsPathRooted(fileName)) { return fileName; }

            return
                lookupPaths
                    .Select(currentDirectoryName => Path.Combine(currentDirectoryName, fileName))
                    .FirstOrDefault(File.Exists);
        }
    }
}