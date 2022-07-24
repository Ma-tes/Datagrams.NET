using DatagramsNet.Logging.Reading.Arguments;
using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Models;
using System.Collections.Immutable;

namespace DatagramsNet.Logging.Reading.Commands
{
    [Command("Test", "Test: [Arguments] [FilePath]")]
    public sealed class TestCommand : Command
    {
        private static readonly ImmutableArray<Option> options = ImmutableArray.Create
            (
            new Option('a'),
            new Option('A'),
            new Option('c'),
            new Option('C')
            );

        private static readonly ImmutableArray<IFactory> arguments = ImmutableArray.Create<IFactory>
            (
            OptionsArgument.Factory(options),
            FileArgument.Factory
            );

        public TestCommand() : base(options, arguments)
        {
        }

        public override ValueTask<CommandResult> ExecuteAsync(Option[] options, object[] arguments)
        {
            var baseResult = base.ExecuteAsync(options, arguments);
            if (baseResult != default)
                return baseResult;

            if (arguments[0] is FileArgument fileArgument)
            {
                var message = $"Your {fileArgument.Name} might be correct if it comes here: {fileArgument.Value}";
                return OkTask(message);
            }
            return FailTask("Wrong syntax.");
        }
    }
}
