using DatagramsNet.Logging.Reading.Arguments;
using System.Collections.Immutable;

namespace DatagramsNet.Logging.Reading.Models
{
    public abstract class Command
    {
        public ImmutableArray<Option> Options { get; }
        public ImmutableArray<IFactory> Arguments { get; }

        public abstract ValueTask<CommandResult> ExecuteAsync(Option[] options, object[] arguments);

        protected Command(ImmutableArray<Option>? options = null, ImmutableArray<IFactory>? arguments = null)
        {
            Options = options ?? ImmutableArray<Option>.Empty;
            Arguments = arguments ?? ImmutableArray<IFactory>.Empty;
        }

        protected static CommandResult Ok(string? message = null)
        {
            return CommandResult.Ok(message);
        }

        protected static ValueTask<CommandResult> OkTask(string? message = null)
        {
            return ValueTask.FromResult(Ok(message));
        }

        protected static CommandResult Fail(string? message = null)
        {
            return CommandResult.Fail(message);
        }

        protected static ValueTask<CommandResult> FailTask(string? message = null)
        {
            return ValueTask.FromResult(Fail(message));
        }
    }
}
