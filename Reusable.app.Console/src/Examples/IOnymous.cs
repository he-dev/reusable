using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.IOnymous;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Mailr;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.IOnymous.Mail;
using Reusable.IOnymous.Mail.Smtp;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.NLog.LayoutRenderers;

//[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(Person))]

namespace Reusable
{
    public static partial class Examples
    {
        public static async Task SendEmailAsync_Smtp()
        {
            var resources =
                ResourceSquid
                    .Builder
                    .AddController(new SmtpController())
                    .Build();

            var context =
                ImmutableContainer
                    .Empty
                    .SetItem(SmtpRequestContext.Host, "localhost")
                    .SetItem(SmtpRequestContext.Port, 25);

            await resources.SendEmailAsync(new Email<EmailSubject, EmailBody>
            {
                From = "console@test.com",
                To = { "me@test.com" },
                CC = { "you@test.com" },
                Subject = new EmailSubject { Value = "How are you?" },
                Body = new EmailBody { Value = "<p>I'm fine!</p>" },
                IsHtml = true,
                Attachments = new Dictionary<string, byte[]>()
            }, context);
        }

        public static async Task SendEmailAsync_Mailr()
        {
            var resources =
                ResourceSquid
                    .Builder
                    .AddController(HttpController.FromBaseUri("http://localhost:7000/api"))
                    .Build();
            await resources.SendEmailAsync
            (
                "v1.0/mailr/messages/plaintext",
                new UserAgent("Console", "v12"),
                new Email.Html
                {
                    From = "console@test.com",
                    To = { "me@test.com" },
                    CC = { "you@test.com" },
                    Subject = "How are you Mailr?",
                    Body = "<p>I'm great!</p>",
                    IsHtml = true,
                    Attachments = new Dictionary<string, byte[]>()
                },
                "providerName"
            );
        }
    }
}