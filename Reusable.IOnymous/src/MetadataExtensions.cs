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
    public interface IAnyNamespace : INamespace
    {
        Encoding Encoding { get; }

        ISet<SoftString> Schemes { get; }

        CancellationToken CancellationToken { get; }
    }

    public interface IProviderNamespace : INamespace
    {
        bool AllowRelativeUri { get; }

        SoftString DefaultName { get; }

        SoftString CustomName { get; }
    }

    public interface IResourceNamespace : INamespace
    {
        MimeType Format { get; }

        Type Type { get; }

        string ActualName { get; }
    }
}