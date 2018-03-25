using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public interface ILogAttachement : IEquatable<ILogAttachement>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] ILog log);
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract  class LogAttachement : ILogAttachement
    {
        protected LogAttachement([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected LogAttachement()
        {
            Name = GetType().Name;
        }

        private string DebuggerDisplay => DebuggerString.Create<LogAttachement>(new { Name = Name.ToString() });

        public SoftString Name { get; }

        public abstract object Compute(ILog log);

        #region IEquatable

        public bool Equals(ILogAttachement other) => AutoEquality<ILogAttachement>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ILogAttachement);

        public override int GetHashCode() => AutoEquality<ILogAttachement>.Comparer.GetHashCode(this);

        #endregion
    }
}