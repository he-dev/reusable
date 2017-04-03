using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace Reusable.Email.Clients.OutlookClient
{
    /// <summary>
    /// Sends emails via Microsoft.Office.Interop.Outlook.
    /// </summary>
    public class OutlookClient : IEmailClient
    {
        public void Send<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody
        {
            if (email == null) throw new ArgumentNullException("email");
            if (email.To == null) throw new ArgumentException("email.To");
            if (email.Subject == null) throw new ArgumentException("email.Subject");
            if (email.Body == null) throw new ArgumentException("email.Body");

            Application app = new Application();
            MailItem mailItem = app.CreateItem(OlItemType.olMailItem);
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
        }
    }

}
