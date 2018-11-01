using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Reusable.Diagnostics.Abstractions;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Diagnostics
{
    public class PhantomExceptionConfig
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        public IList<IPhantomExceptionTrigger> Read(Stream stream)
        {
            return JsonSerializer.Deserialize<IList<IPhantomExceptionTrigger>>(stream);
        }
    }
}