using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Indexes;
using DatagramsNet.Logging.Reading.Interfaces;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Logging.Reading.Commands
{
    [Command("Test", "Test: [Arugments] [FilePath]")]
    public sealed class TestCommand : ICommand
    {
        public Argument[] Arguments => new Argument[]
        {
            new Argument('a'),
            new Argument('A'),
            new Argument('c'),
            new Argument('C'),
        };

        public object[] Indexes => new object[]
        {
            new ArgumentIndex() {Command = this },
            new FileIndex(),
        };

        public async Task<string> ExecuteCommand(Argument[] args, object[] indexes)
        {
            if (indexes[0] is FileIndex newIndex)
            {
                var message = $"Your {newIndex.Name} is maybe rigth if comes here: {newIndex.Value}";
                return message;
            }
            else
                await ServerLogger.Log<ErrorPrefix>("Sorry but your syntax is wrong", TimeFormat.HALF);
            return string.Empty;
        }
    }
}
