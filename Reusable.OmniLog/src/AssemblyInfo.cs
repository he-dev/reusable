using System.Diagnostics;
using System.Runtime.CompilerServices;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(Scalar))]
[assembly: InternalsVisibleTo("Reusable.Tests")]