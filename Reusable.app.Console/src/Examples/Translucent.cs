using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Models;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.Mailr;
using Reusable.Utilities.Mailr.Models;
using Reusable.Utilities.NLog.LayoutRenderers;

//[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(Person))]

namespace Reusable
{
    public static partial class Examples
    {
        public static async Task SendEmailViaSmtp()
        {
            var resources = ResourceRepository.Create(c => c.AddSmtp());

            var context =
                ImmutableContainer
                    .Empty
                    .SetItem(SmtpRequest.Host, "localhost")
                    .SetItem(SmtpRequest.Port, 25);

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

        public static async Task SendEmailViaMailr()
        {
            var resources = ResourceRepository.Create(c => c.AddHttp("http://localhost:7000/api", ImmutableContainer.Empty.UpdateItem(ResourceController.Tags, x => x.Add("Mailr"))));

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
                }
            );
        }
    }
}