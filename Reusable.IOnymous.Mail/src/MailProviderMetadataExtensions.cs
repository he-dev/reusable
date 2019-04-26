using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

#if NET47
using System.Net.Configuration;
#endif

namespace Reusable.IOnymous
{
    [PublicAPI]
    public static class MailProviderMetadataExtensions
    {
        public static Metadata<MailProvider> Mail(this Metadata metadata)
        {
            return metadata.Scope<MailProvider>();
        }

        public static Metadata Mail(this Metadata metadata, ConfigureMetadataScopeCallback<MailProvider> scope)
        {
            return metadata.Scope(scope);
        }

        public static IEnumerable<string> To(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(Enumerable.Empty<string>());
        }

        public static Metadata<MailProvider> To(this Metadata<MailProvider> scope, IEnumerable<string> to)
        {
            return scope.Value.SetItemByCallerName(to);
        }

        // --- 

        public static IEnumerable<string> CC(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(Enumerable.Empty<string>());
        }

        public static Metadata<MailProvider> CC(this Metadata<MailProvider> scope, IEnumerable<string> cc)
        {
            return scope.Value.SetItemByCallerName(cc);
        }

        // ---

        public static string From(this Metadata<MailProvider> scope)
        {
#if NETCOREAPP2_2
            return scope.Value.GetItemByCallerName(string.Empty);
#endif
#if NET47
            return scope.Value.GetItemByCallerName(Default());

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
#endif
        }

        public static Metadata<MailProvider> From(this Metadata<MailProvider> scope, string from)
        {
            return scope.Value.SetItemByCallerName(from);
        }

        // ---

        public static string Subject(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(string.Empty);
        }

        public static Metadata<MailProvider> Subject(this Metadata<MailProvider> scope, string subject)
        {
            return scope.Value.SetItemByCallerName(subject);
        }

        // ---

        public static Encoding SubjectEncoding(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(Encoding.UTF8);
        }

        public static Metadata<MailProvider> SubjectEncoding(this Metadata<MailProvider> scope, string subjectEncoding)
        {
            return scope.Value.SetItemByCallerName(subjectEncoding);
        }

        public static Dictionary<string, byte[]> Attachments(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(new Dictionary<string, byte[]>());
        }

        public static Metadata<MailProvider> Attachments(this Metadata<MailProvider> scope, Dictionary<string, byte[]> attachments)
        {
            return scope.Value.SetItemByCallerName(attachments);
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

        public static Encoding BodyEncoding(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(Encoding.UTF8);
        }

        public static Metadata<MailProvider> BodyEncoding(this Metadata<MailProvider> scope, string bodyEncoding)
        {
            return scope.Value.SetItemByCallerName(bodyEncoding);
        }

        // ---

        public static bool IsHtml(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(true);
        }

        public static Metadata<MailProvider> IsHtml(this Metadata<MailProvider> scope, bool isBodyHtml)
        {
            return scope.Value.SetItemByCallerName(isBodyHtml);
        }

        // ---

        public static bool IsHighPriority(this Metadata<MailProvider> scope)
        {
            return scope.Value.GetItemByCallerName(false);
        }

        public static Metadata<MailProvider> IsHighPriority(this Metadata<MailProvider> scope, bool isHighPriority)
        {
            return scope.Value.SetItemByCallerName(isHighPriority);
        }

        // ---
    }

    
}