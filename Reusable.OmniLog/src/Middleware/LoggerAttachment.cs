using System.Collections;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2.Middleware
{
    /// <summary>
    /// Adds calculated properties to the log.
    /// </summary>
    public class LoggerAttachment : LoggerMiddleware, IEnumerable<ILogAttachment>
    {
        public static readonly string LogItemTag = "Attachment";
        
        private readonly IList<ILogAttachment> _attachments = new List<ILogAttachment>();

        public LoggerAttachment() : base(true) { }

        protected override void InvokeCore(Abstractions.v2.Log request)
        {
            foreach (var attachment in _attachments)
            {
                request.SetItem((attachment.Name, LogItemTag), attachment.Compute(request));
            }

            Next?.Invoke(request);
        }

        public void Add(ILogAttachment attachment) => _attachments.Add(attachment);

        public IEnumerator<ILogAttachment> GetEnumerator() => _attachments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_attachments).GetEnumerator();
    }
}