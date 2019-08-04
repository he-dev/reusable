using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.v2;

namespace Reusable.OmniLog.SemanticExtensions.v2.Middleware
{
    using Reusable.OmniLog.Abstractions.v2;
    using Reusable.OmniLog.SemanticExtensions.v2;

    public class LoggerAbstraction : LoggerMiddleware
    {
        public LoggerAbstraction() : base(true) { }

        protected override void InvokeCore(ILog request)
        {
            if (request.TryGetValue(nameof(SemanticExtensions.AbstractionProperties), out var obj) && obj is IAbstractionContext context)
            {
                request.SetItem(AbstractionProperties.Layer, context.Values[AbstractionProperties.Layer]);
                request.SetItem(AbstractionProperties.Category, context.Values[AbstractionProperties.Category]);
                //request.SetItem(AbstractionProperties.Identifier, name);
                request.AttachSerializable(AbstractionProperties.Snapshot, context.Values[AbstractionProperties.Snapshot]);
                if (context.Values.TryGetValue(AbstractionProperties.Because, out var because) && because is string message)
                {
                    request.Message(message);
                }
            }

            Next?.Invoke(request);
        }
    }
}