using DatagramsNet.Logging.Reading.Arguments;
using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Models;
using System.Collections.Immutable;

namespace DatagramsNet.Logging.Reading.Commands
{
    [Command("Help", "Help: [Command]")]
    public sealed class HelpCommand : Command, ICommandAction
    {
        public Action? CommandAction { get; set; }

        private static readonly ImmutableArray<IFactory> arguments = ImmutableArray.Create<IFactory>
        (
            CommandArgument.Factory
        );

        public HelpCommand() : base(arguments: arguments)
        {
        }

        public override ValueTask<CommandResult> ExecuteAsync(Option[] options, object[] arguments)
        {
            if (arguments[0] is CommandArgument commandIndex)
            {
                CommandAction?.Invoke();
                return OkTask($"{commandIndex.Name}: {commandIndex.Value}");
            }
            return FailTask("This command was not found.");
        }
    }
}
