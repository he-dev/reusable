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
    public interface IAnySession : ISessionScope
    {
        Encoding Encoding { get; }

        ISet<SoftString> Schemes { get; }

        CancellationToken CancellationToken { get; }
    }

    public interface IProviderSession : ISessionScope
    {
        bool AllowRelativeUri { get; }

        SoftString DefaultName { get; }

        SoftString CustomName { get; }
    }

    public interface IResourceSession : ISessionScope
    {
        MimeType Format { get; }

        Type Type { get; }

        string ActualName { get; }
    }
}