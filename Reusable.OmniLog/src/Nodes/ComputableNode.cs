using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds computed properties to the log.
    /// </summary>
    public class ComputableNode : LoggerNode, IEnumerable<IComputable>
    {
        private readonly ISet<IComputable> _computables = new HashSet<IComputable>();
        private readonly ISet<IComputable> _disabled = new HashSet<IComputable>();

        public ComputableNode() : base(true) { }

        public override bool IsActive => base.IsActive && _computables.Any();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var computable in _computables.Except(_disabled))
            {
                request.SetItem(computable.Name, default, computable.Compute(request));

                // todo - to catch or not to catch?

//                try
//                {
//                }
//// Don't use this item when it failed to prevent further exceptions.
//                catch (Exception ex)
//                {
//#if DEBUG
//                    throw;
//#else
//                    _disabled.Add(attachment);
//#endif
//                }
            }

            Next?.Invoke(request);
        }

        public void Add(IComputable computable) => _computables.Add(computable);

        public IEnumerator<IComputable> GetEnumerator() => _computables.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_computables).GetEnumerator();
    }
}