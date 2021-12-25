﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Octopus;
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
        public static async Task SendEmailOverSmtp()
        {
            var resources = new Resource(new[] { new SmtpController() });

            await resources.SendEmailAsync(new Email<EmailSubject, EmailBody>
            {
                From = "console@test.com",
                To = { "me@test.com" },
                CC = { "you@test.com" },
                Subject = new EmailSubject { Value = "How are you?" },
                Body = new EmailBody { Value = "<p>I'm fine!</p>" },
                IsHtml = true,
                Attachments = new Dictionary<string, byte[]>()
            }, smtp =>
            {
                smtp.Host = "localhost";
                smtp.Port = 25;
            });
        }

        public static async Task SendEmailOverMailr()
        {
            var resources = new Resource(new[]
            {
                new HttpController
                {
                    CreateHttpClient = () => new HttpClient
                    {
                        BaseAddress = new Uri("http://localhost:7000/api")
                    },
                    Name = "Mailr"
                }
            });
            
            await resources.SendEmailAsync
            (
                "v1.0/mailr/messages/plaintext",
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
                http => { http.HeaderActions.Add(headers => headers.UserAgent("Console", "v16")); }
            );
        }
    }
}