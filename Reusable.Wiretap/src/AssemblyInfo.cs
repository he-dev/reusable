using System.Diagnostics;
using System.Runtime.CompilerServices;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(PropertyService))]
[assembly: InternalsVisibleTo("Reusable.Tests")]