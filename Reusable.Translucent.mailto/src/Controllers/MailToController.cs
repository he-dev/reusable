using System.IO;
using System.Threading.Tasks;

namespace Reusable.Translucent.Controllers
{
    [Handles(typeof(MailToRequest))]
    public abstract class MailToController : ResourceController
    {
        protected MailToController(ControllerName controllerName) : base(controllerName, UriSchemes.Known.MailTo) { }

        protected static async Task<string> ReadBodyAsync(Stream value, MailToRequest request)
        {
            using var bodyReader = new StreamReader(value, request.Encoding);
            return await bodyReader.ReadToEndAsync();
        }
    }
}