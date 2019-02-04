using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Reusable.Deception.Abstractions;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Deception
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