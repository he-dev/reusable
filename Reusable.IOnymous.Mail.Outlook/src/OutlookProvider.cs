using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public class OutlookProvider : MailProvider
    {
        public OutlookProvider(Metadata metadata) : base(metadata.AllowRelativeUri(true)) { }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            var mail = metadata.Mail();

            var app = new Application();
            var mailItem = (dynamic)app.CreateItem(OlItemType.olMailItem);

            mailItem.Subject = mail.Subject();
            mailItem.To = mail.To().Where(Conditional.IsNotNullOrEmpty).Join(";");

            if (mail.IsHtml())
            {
                mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
                mailItem.HTMLBody = await ReadBodyAsync(value, metadata);
            }
            else
            {
                mailItem.BodyFormat = OlBodyFormat.olFormatPlain;
                mailItem.Body = await ReadBodyAsync(value, metadata);
            }

            mailItem.Importance = mail.IsHighPriority() ? OlImportance.olImportanceHigh : OlImportance.olImportanceNormal;
            //mailItem.Display(false);
            mailItem.Send();

            return new InMemoryResourceInfo(uri, Metadata.Empty);
        }
    }
}