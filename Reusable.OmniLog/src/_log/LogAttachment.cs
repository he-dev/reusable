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

namespace Reusable.OmniLog
{
    public interface ILogAttachment : IEquatable<ILogAttachment>
    {
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        object Compute([NotNull] ILog log);
    }
   
    public abstract class LogAttachment : ILogAttachment
    {
        protected LogAttachment([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected LogAttachment()
        {
            Name = GetType().Name;
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder => { builder.DisplayMember(x => x.Name); });

        public SoftString Name { get; }

        public abstract object Compute(ILog log);

        #region IEquatable

        public bool Equals(ILogAttachment other) => AutoEquality<ILogAttachment>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ILogAttachment);

        public override int GetHashCode() => AutoEquality<ILogAttachment>.Comparer.GetHashCode(this);

        #endregion
    }
}