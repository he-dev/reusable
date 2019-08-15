using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    public abstract class MailController : ResourceController
    {
        protected MailController(IImmutableContainer metadata)
            : base(metadata.UpdateItem(ResourceControllerProperties.Schemes, s => s.Add(UriSchemes.Known.MailTo))) { }

        protected async Task<string> ReadBodyAsync(Stream value, IImmutableContainer metadata)
        {
            using (var bodyReader = new StreamReader(value, metadata.GetItemOrDefault(MailRequestMetadata.BodyEncoding, Encoding.UTF8)))
            {
                return await bodyReader.ReadToEndAsync();
            }
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(MailRequestMetadata))]
    public class MailRequestMetadata : SelectorBuilder<MailRequestMetadata>
    {
        public static Selector<string> From = Select(() => From);

        public static Selector<IList<string>> To = Select(() => To);

        public static Selector<IList<string>> CC = Select(() => CC);

        public static Selector<string> Subject = Select(() => Subject);

        public static Selector<IDictionary<string, byte[]>> Attachments = Select(() => Attachments);

        public static Selector<Encoding> BodyEncoding = Select(() => BodyEncoding);

        public static Selector<bool> IsHtml = Select(() => IsHtml);

        public static Selector<bool> IsHighPriority = Select(() => IsHighPriority);
    }
}