using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Linq.Custom;

namespace Reusable.OmniLog.Utilities
{
    public abstract class RxFilter : IEnumerable<Reusable.OmniLog.ILogRx>
    {
        private readonly IEnumerable<Reusable.OmniLog.ILogRx> _receivers;

        protected RxFilter(IEnumerable<Reusable.OmniLog.ILogRx> receivers)
        {
            _receivers = receivers;
        }

        protected abstract bool Predicate(ILogRx rx);

        public IEnumerator<ILogRx> GetEnumerator()
        {
            return _receivers.Where(Predicate).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class AppConfigRxFilter : RxFilter
    {
        private readonly IImmutableSet<SoftString> _filters;

        public AppConfigRxFilter(params ILogRx[] receivers)
            : base(receivers)
        {
            var filter = ConfigurationManager.AppSettings["omnilog:RxFilter"] ?? string.Empty; // NLogRx, VaultRx
            _filters = filter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(SoftString.Create).ToImmutableHashSet();
        }

        protected override bool Predicate(ILogRx rx)
        {
            return _filters.Empty() || _filters.Contains(rx.GetType().Name);
        }
    }
}
