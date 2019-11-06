using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Switch : ScalarExtension<object>
    {
        public Switch() : this(default, nameof(Switch)) { }

        protected Switch(ILogger logger, SoftString name) : base(logger, name) { }

        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        public IEnumerable<SwitchCase> Cases { get; set; }

        public IExpression Default { get; set; }

        protected override object InvokeAsValue(IImmutableContainer context)
        {
            var value = This(context).Invoke(context);

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                var scope = context.BeginScopeWithThisOuter(value);

                switch (switchCase.When)
                {
                    case IConstant constant:
                        if (EqualityComparer<object>.Default.Equals(value.Value, constant.Value))
                        {
                            var bodyResult = switchCase.Body.Invoke(scope);
                            return bodyResult.Value;
                        }

                        break;
                    case { } expression:
                        if (expression.Invoke(context) is var whenResult && whenResult.Value<bool>())
                        {
                            var bodyResult = switchCase.Body.Invoke(scope);
                            return bodyResult.Value;
                        }

                        break;
                }
            }

            if (Default is IConstant @default)
            {
                return ("Switch.Default", @default.Value);
            }

            return
                (Default ?? new Throw
                    {
                        Name = "SwitchValueOutOfRange",
                        Message = Constant.FromValue("Message", "Default value not specified.")
                    }
                ).Invoke(context);
        }
    }

    public class SwitchCase
    {
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [JsonRequired]
        public IExpression When { get; set; }

        [JsonRequired]
        public IExpression Body { get; set; }
    }

    //    [UseType]
    //    [UseMember]
    //    [TrimEnd("I")]
    //    [TrimStart("Meta")]
    //    public interface ISwitchMeta : INamespace
    //    {
    //        object Value { get; }
    //    }
}