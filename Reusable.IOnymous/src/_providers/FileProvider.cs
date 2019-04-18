using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public abstract class FileProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "file";

        protected FileProvider([NotNull] Metadata metadata)
            : base(new SoftString[] { DefaultScheme }, metadata)
        {
        }
    }
}