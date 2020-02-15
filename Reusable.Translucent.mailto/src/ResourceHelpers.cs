using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Reusable.Translucent.Models;

namespace Reusable.Translucent
{
    [PublicAPI]
    public static class ResourceHelpers
    {
        public static async Task<Response> SendEmailAsync
        (
            this IResource resource,
            IEmail<IEmailSubject, IEmailBody> email,
            Action<SmtpRequest>? requestAction = default
        )
        {
            return await resource.CreateAsync<SmtpRequest>(string.Empty, email.Body.Value, request =>
            {
                request.From = email.From;
                request.To = email.To;
                request.CC = email.CC;
                request.Subject = email.Subject.Value;
                request.Attachments = email.Attachments;
                request.From = email.From;
                request.IsHtml = email.IsHtml;
                request.IsHighPriority = email.IsHighPriority;
                request.Pipe(requestAction);
            });
        }
    }

    
}