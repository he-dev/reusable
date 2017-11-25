using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace Reusable.Net.Mail
{
    /// <summary>
    /// Sends emails via Microsoft.Office.Interop.Outlook.
    /// </summary>
    public class OutlookClient : EmailClient
    {
        protected override Task SendAsyncCore<TSubject, TBody>(IEmail<TSubject, TBody> email)
        {
            var app = new Application();
            var mailItem = app.CreateItem(OlItemType.olMailItem);

            mailItem.Subject = email.Subject.ToString();
            mailItem.To = email.To;

            if (email.Body.IsHtml)
            {
                mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
                mailItem.HTMLBody = email.Body.ToString();
            }
            else
            {
                mailItem.BodyFormat = OlBodyFormat.olFormatPlain;
                mailItem.Body = email.Body.ToString();
            }

            mailItem.Importance = email.HighPriority ? OlImportance.olImportanceHigh : OlImportance.olImportanceNormal;
            //mailItem.Display(false);
            mailItem.Send();

            return Task.CompletedTask;
        }
    }

}
