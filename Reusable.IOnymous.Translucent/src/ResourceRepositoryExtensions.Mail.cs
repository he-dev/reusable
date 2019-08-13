using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous.Config;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Formatting;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.IOnymous.Mail;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    // Provides CRUD APIs.
    public static partial class ResourceRepositoryExtensions
    {
        public static async Task<IResource> SendEmailAsync
        (
            this IResourceRepository resourceRepository,
            IEmail<IEmailSubject, IEmailBody> email,
            IImmutableContainer context = default
        )
        {
            context =
                context
                    .ThisOrEmpty()
                    .SetItem(MailRequestContext.From, email.From)
                    .SetItem(MailRequestContext.To, email.To)
                    .SetItem(MailRequestContext.CC, email.CC)
                    .SetItem(MailRequestContext.Subject, email.Subject.Value)
                    .SetItem(MailRequestContext.Attachments, email.Attachments)
                    .SetItem(MailRequestContext.From, email.From)
                    .SetItem(MailRequestContext.IsHtml, email.IsHtml)
                    .SetItem(MailRequestContext.IsHighPriority, email.IsHighPriority);

            return
                await resourceRepository.InvokeAsync(new Request.Post($"{UriSchemes.Known.MailTo}:dummy@email.com")
                {
                    Context = context,
                    Body = email.Body.Value,
                    CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, email.Body.Encoding)
                });
        }
    }
}