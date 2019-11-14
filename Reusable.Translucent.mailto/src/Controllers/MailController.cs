using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    public abstract class MailController : ResourceController
    {
        protected MailController(IImmutableContainer metadata)
            : base(metadata.UpdateItem(Schemes, s => s.Add(UriSchemes.Known.MailTo))) { }

        protected async Task<string> ReadBodyAsync(Stream value, IImmutableContainer metadata)
        {
            using (var bodyReader = new StreamReader(value, metadata.GetItemOrDefault(Request.Encoding, Encoding.UTF8)))
            {
                return await bodyReader.ReadToEndAsync();
            }
        }

        #region Properties

        private static readonly From<MailController> This;
        
        public static Selector<string> From = This.Select(() => From);

        public static Selector<IList<string>> To = This.Select(() => To);

        public static Selector<IList<string>> CC = This.Select(() => CC);

        public static Selector<string> Subject = This.Select(() => Subject);

        public static Selector<IDictionary<string, byte[]>> Attachments = This.Select(() => Attachments);

        //public static Selector<Encoding> BodyEncoding = This.Select(() => BodyEncoding);

        public static Selector<bool> IsHtml = This.Select(() => IsHtml);

        public static Selector<bool> IsHighPriority = This.Select(() => IsHighPriority);

        #endregion
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(MailRequestMetadata))]
    public class MailRequestMetadata : SelectorBuilder<MailRequestMetadata>
    {
    }
}