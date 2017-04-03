using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Email
{
    public interface IEmailClient
    {
        void Send<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody;
    }
}
