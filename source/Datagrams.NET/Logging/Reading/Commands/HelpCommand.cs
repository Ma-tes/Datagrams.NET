using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Arguments;
using DatagramsNet.Logging.Reading.Models;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Logging.Reading.Commands
{
    [Command("Help", "Help: [Command]")]
    public sealed class HelpCommand : ICommand, ICommandAction
    {
        public Option[]? Options => null;

        public Action? CommandAction { get; set; }

        public object[] Arguments => new object[]
        {
            new CommandArgument()
        };

        public async Task<string?> ExecuteCommandAsync(Option[] args, object[] indexes)
        {
            if (indexes[0] is CommandArgument commandIndex)
            {
                CommandAction?.Invoke();
                return $"{commandIndex.Name}: {commandIndex.Value}";
            }
            await ServerLogger.LogAsync<ErrorPrefix>("This command was found.", TimeFormat.Half);
            return null;
        }
    }
}
