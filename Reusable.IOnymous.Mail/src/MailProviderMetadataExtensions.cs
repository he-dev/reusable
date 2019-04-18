using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    
    [PublicAPI]
    public static class MailProviderMetadataExtensions
    {
        public static MetadataScope<MailProvider> Mail(this Metadata metadata)
        {
            return metadata.For<MailProvider>();
        }

        public static Metadata Mail(this Metadata metadata, ConfigureMetadataScopeCallback<MailProvider> scope)
        {
            return metadata.For(scope);
        }

        public static IEnumerable<string> To(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(Enumerable.Empty<string>());
        }

        public static MetadataScope<MailProvider> To(this MetadataScope<MailProvider> scope, IEnumerable<string> to)
        {
            return scope.Metadata.SetItemWithCallerName(to);
        }

        // --- 

        public static IEnumerable<string> CC(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(Enumerable.Empty<string>());
        }

        public static MetadataScope<MailProvider> CC(this MetadataScope<MailProvider> scope, IEnumerable<string> cc)
        {
            return scope.Metadata.SetItemWithCallerName(cc);
        }

        // ---

        public static string From(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(Default());

            string Default()
            {
                const string mailSectionGroupName = "system.net/mailSettings";
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mailSettingsSectionGroup = configuration.GetSectionGroup(mailSectionGroupName) as MailSettingsSectionGroup;
                if (mailSettingsSectionGroup?.Smtp?.From is null)
                {
                    throw new InvalidOperationException
                    (
                        $"You didn't specify {nameof(From)} email and there is no default value in the '{mailSectionGroupName}' section in the app.config."
                    );
                }

                return mailSettingsSectionGroup.Smtp.From;
            }
        }

        public static MetadataScope<MailProvider> From(this MetadataScope<MailProvider> scope, string from)
        {
            return scope.Metadata.SetItemWithCallerName(from);
        }

        // ---

        public static string Subject(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(string.Empty);
        }

        public static MetadataScope<MailProvider> Subject(this MetadataScope<MailProvider> scope, string subject)
        {
            return scope.Metadata.SetItemWithCallerName(subject);
        }

        // ---

        public static Encoding SubjectEncoding(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(Encoding.UTF8);
        }

        public static MetadataScope<MailProvider> SubjectEncoding(this MetadataScope<MailProvider> scope, string subjectEncoding)
        {
            return scope.Metadata.SetItemWithCallerName(subjectEncoding);
        }

        // todo - get/set Attachments Dictionary<string, byte[]>

        public static Dictionary<string, byte[]> Attachments(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(default(Dictionary<string, byte[]>));
        }

        public static MetadataScope<MailProvider> Attachments(this MetadataScope<MailProvider> scope, Dictionary<string, byte[]> attachments)
        {
            return scope.Metadata.SetItemWithCallerName(attachments);
        }

        // ---

        //        public static string Body(this ResourceMetadataScope<MailProvider> scope)
        //        {
        //            return scope.Metadata.GetValueOrDefault(string.Empty);            
        //        }
        //
        //        public static ResourceMetadataScope<MailProvider> Body(this ResourceMetadataScope<MailProvider> scope, string body)
        //        {
        //            return scope.Metadata.SetItemAuto(body);
        //        }

        // ---

        public static Encoding BodyEncoding(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(Encoding.UTF8);
        }

        public static MetadataScope<MailProvider> BodyEncoding(this MetadataScope<MailProvider> scope, string bodyEncoding)
        {
            return scope.Metadata.SetItemWithCallerName(bodyEncoding);
        }

        // ---

        public static bool IsHtml(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(true);
        }

        public static MetadataScope<MailProvider> IsHtml(this MetadataScope<MailProvider> scope, bool isBodyHtml)
        {
            return scope.Metadata.SetItemWithCallerName(isBodyHtml);
        }

        // ---

        public static bool IsHighPriority(this MetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(false);
        }

        public static MetadataScope<MailProvider> IsHighPriority(this MetadataScope<MailProvider> scope, bool isHighPriority)
        {
            return scope.Metadata.SetItemWithCallerName(isHighPriority);
        }

        // ---
    }
}