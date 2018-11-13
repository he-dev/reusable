using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Reusable.IO
{
    [PublicAPI]
    public interface IFileInfo : IEquatable<IFileInfo>, IEquatable<string>
    {
        [NotNull]
        string Path { get; }

        [NotNull]
        string Name { get; }

        bool Exists { get; }

        long Length { get; }

        DateTime ModifiedOn { get; }        

        bool IsDirectory { get; }

        [NotNull]
        Stream CreateReadStream();
    }
}