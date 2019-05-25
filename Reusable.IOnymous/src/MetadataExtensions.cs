using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    [TypeMemberKeyFactory]
    [RemovePrefix("I")]
    [RemoveSuffix("Meta")]
    public interface IAnyMeta : INamespace
    {
        Encoding Encoding { get; }

        ISet<SoftString> Schemes { get; }

        CancellationToken CancellationToken { get; }
    }

    [TypeMemberKeyFactory]
    [RemovePrefix("I")]
    [RemoveSuffix("Meta")]
    public interface IProviderMeta : INamespace
    {
        bool AllowRelativeUri { get; }

        SoftString DefaultName { get; }

        SoftString CustomName { get; }
    }

    [TypeMemberKeyFactory]
    [RemovePrefix("I")]
    [RemoveSuffix("Meta")]
    public interface IResourceMeta : INamespace
    {
        MimeType Format { get; }

        Type Type { get; }

        string ActualName { get; }
    }
}