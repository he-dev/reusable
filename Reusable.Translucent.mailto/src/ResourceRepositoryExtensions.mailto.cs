using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Models;

namespace Reusable.Translucent
{
    [PublicAPI]
    public static class ResourceRepositoryExtensions
    {
        public static async Task<Response> SendEmailAsync
        (
            this IResourceRepository resourceRepository,
            IEmail<IEmailSubject, IEmailBody> email,
            Action<SmtpRequest>? requestAction = default
        )
        {
            return await resourceRepository.PostAsync<SmtpRequest>($"{UriSchemes.Known.MailTo}:john.doe@email.com", email.Body.Value, request =>
            {
                request.From = email.From;
                request.To = email.To;
                request.CC = email.CC;
                request.Subject = email.Subject.Value;
                request.Attachments = email.Attachments;
                request.From = email.From;
                request.IsHtml = email.IsHtml;
                request.IsHighPriority = email.IsHighPriority;
                requestAction?.Invoke(request);
            });
        }
    }

    [PublicAPI]
    public abstract class MailToRequest : Request
    {
        public string From { get; set; }
        public List<string> To { get; set; } = new List<string>();
        public List<string> CC { get; set; } = new List<string>();
        public string Subject { get; set; }
        public Dictionary<string, byte[]> Attachments { get; set; } = new Dictionary<string, byte[]>();
        public bool IsHtml { get; set; }
        public bool IsHighPriority { get; set; }
    }
}