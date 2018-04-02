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
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(LogAttachement))]

namespace Reusable.OmniLog
{
    public interface ILogAttachement : IEquatable<ILogAttachement>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] ILog log);
    }
   
    public abstract class LogAttachement : ILogAttachement
    {
        protected LogAttachement([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected LogAttachement()
        {
            Name = GetType().Name;
        }

        private string DebuggerDisplay() => DebuggerDisplayHelper<LogAttachement>.ToString(this, builder => { builder.Property(x => x.Name); });

        public SoftString Name { get; }

        public abstract object Compute(ILog log);

        #region IEquatable

        public bool Equals(ILogAttachement other) => AutoEquality<ILogAttachement>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ILogAttachement);

        public override int GetHashCode() => AutoEquality<ILogAttachement>.Comparer.GetHashCode(this);

        #endregion
    }
}