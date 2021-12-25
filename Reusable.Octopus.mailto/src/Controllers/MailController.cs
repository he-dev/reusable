using System.IO;
using System.Threading.Tasks;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    public abstract class MailToController<T> : ResourceController<T> where T : MailRequest
    {
        protected static async Task<string> ReadBodyAsync(Stream value, MailRequest request)
        {
            using var bodyReader = new StreamReader(value.Rewind(), request.Encoding);
            return await bodyReader.ReadToEndAsync();
        }
    }
}