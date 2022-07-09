using System.Collections.Immutable;

namespace DatagramsNet.Logging.Reading.Models
{
    public abstract class Command
    {
        public ImmutableArray<Option> Options { get; }
        public ImmutableArray<object> Arguments { get; }

        public abstract ValueTask<CommandResult> ExecuteAsync(Option[] options, object[] arguments);

        protected static CommandResult Ok(string message)
        {
            return CommandResult.Ok(message);
        }

        protected static CommandResult Fail(string? message = null)
        {
            return CommandResult.Fail(message);
        }
    }
}
