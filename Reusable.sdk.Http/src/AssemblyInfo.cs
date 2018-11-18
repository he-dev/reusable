using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.sdk.Http;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(IRestClient))]
[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(PartialUriBuilder))]