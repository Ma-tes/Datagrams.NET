using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Arguments;
using DatagramsNet.Logging.Reading.Models;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Logging.Reading.Commands
{
    [Command("Test", "Test: [Arguments] [FilePath]")]
    public sealed class TestCommand : ICommand
    {
        public Option[] Options => new Option[]
        {
            new Option('a'),
            new Option('A'),
            new Option('c'),
            new Option('C'),
        };

        public object[] Arguments => new object[]
        {
            new OptionsArgument() { Command = this },
            new FileArgument(),
        };

        public async Task<string?> ExecuteCommandAsync(Option[] options, object[] arguments)
        {
            if (arguments[0] is FileArgument fileArgument)
            {
                var message = $"Your {fileArgument.Name} is maybe rigth if comes here: {fileArgument.Value}";
                return message;
            }
            else
                await ServerLogger.LogAsync<ErrorPrefix>("Sorry but your syntax is wrong", TimeFormat.Half);
            return string.Empty;
        }
    }
}
