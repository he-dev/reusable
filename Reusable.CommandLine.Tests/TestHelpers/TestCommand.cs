using System;

namespace Candle.Tests.TestHelpers
{
    internal class TestCommand<TCommand> : Command where TCommand : Command, new()
    {
        public Func<TCommand, int> ExecuteFunc { get; set; }

        public override int Execute()
        {
            return ExecuteFunc?.Invoke((TCommand)(Command)this) ?? 404;
        }
    }
}
