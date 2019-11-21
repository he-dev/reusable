using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    public abstract class MailController : ResourceController
    {
        protected MailController(string? id) : base(id, UriSchemes.Known.MailTo) { }

        protected static async Task<string> ReadBodyAsync(Stream value, MailToRequest request)
        {
            using var bodyReader = new StreamReader(value, request.Encoding);
            return await bodyReader.ReadToEndAsync();
        }
    }
}