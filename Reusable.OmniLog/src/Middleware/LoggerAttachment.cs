using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    /// <summary>
    /// Adds calculated properties to the log.
    /// </summary>
    public class LoggerAttachment : LoggerMiddleware, IEnumerable<ILogAttachment>
    {
        private readonly ISet<ILogAttachment> _attachments = new HashSet<ILogAttachment>();
        private readonly ISet<ILogAttachment> _disabled = new HashSet<ILogAttachment>();

        public LoggerAttachment() : base(true) { }

        public override bool IsActive => base.IsActive && _attachments.Any();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var attachment in _attachments.Except(_disabled))
            {
                try
                {
                    request.SetItem(attachment.Name, default, attachment.Compute(request));
                }
                catch (Exception ex)
                {
                #if DEBUG
                    throw;
                #else
                    _disabled.Add(attachment);
                #endif
                }
            }

            Next?.Invoke(request);
        }

        public void Add(ILogAttachment attachment) => _attachments.Add(attachment);

        public IEnumerator<ILogAttachment> GetEnumerator() => _attachments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_attachments).GetEnumerator();
    }
}