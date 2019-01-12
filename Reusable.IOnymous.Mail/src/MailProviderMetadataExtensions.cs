using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public static class MailProviderMetadataExtensions
    {
        public static IEnumerable<string> To(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Enumerable.Empty<string>());
        }

        public static ResourceMetadataScope<MailProvider> To(this ResourceMetadataScope<MailProvider> scope, IEnumerable<string> to)
        {
            return scope.Metadata.SetItemAuto(to);
        }
        
        // --- 
        
        public static IEnumerable<string> CC(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Enumerable.Empty<string>());
        }

        public static ResourceMetadataScope<MailProvider> CC(this ResourceMetadataScope<MailProvider> scope, IEnumerable<string> cc)
        {
            return scope.Metadata.SetItemAuto(cc);
        }

        // ---

        public static string From(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Default());

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

        public static ResourceMetadataScope<MailProvider> From(this ResourceMetadataScope<MailProvider> scope, string from)
        {
            return scope.Metadata.SetItemAuto(from);
        }

        // ---
        
        public static string Subject(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(string.Empty);            
        }

        public static ResourceMetadataScope<MailProvider> Subject(this ResourceMetadataScope<MailProvider> scope, string subject)
        {
            return scope.Metadata.SetItemAuto(subject);
        }

        // ---
        
        public static Encoding SubjectEncoding(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Encoding.UTF8);            
        }

        public static ResourceMetadataScope<MailProvider> SubjectEncoding(this ResourceMetadataScope<MailProvider> scope, string subjectEncoding)
        {
            return scope.Metadata.SetItemAuto(subjectEncoding);
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
        
        public static Encoding BodyEncoding(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(Encoding.UTF8);            
        }

        public static ResourceMetadataScope<MailProvider> BodyEncoding(this ResourceMetadataScope<MailProvider> scope, string bodyEncoding)
        {
            return scope.Metadata.SetItemAuto(bodyEncoding);
        }

        // ---
        
        public static bool IsHtml(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(true);            
        }

        public static ResourceMetadataScope<MailProvider> IsHtml(this ResourceMetadataScope<MailProvider> scope, bool isBodyHtml)
        {
            return scope.Metadata.SetItemAuto(isBodyHtml);
        }

        // ---
        
        public static bool IsHighPriority(this ResourceMetadataScope<MailProvider> scope)
        {
            return scope.Metadata.GetValueOrDefault(false);            
        }

        public static ResourceMetadataScope<MailProvider> IsHighPriority(this ResourceMetadataScope<MailProvider> scope, bool isHighPriority)
        {
            return scope.Metadata.SetItemAuto(isHighPriority);
        }

        // ---
    }
}