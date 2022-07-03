using DatagramsNet.Datagrams.NET.Logger.Reader.Attributes;
using DatagramsNet.Datagrams.NET.Logger.Reader.Indexes;
using DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces;
using DatagramsNet.Datagrams.NET.Prefixes;

namespace DatagramsNet.Datagrams.NET.Logger.Reader.Commands
{
    [Command("Help", "Help: [Command]")]
    public sealed class HelpCommand : ICommand, ICommandAction
    {
        public Argument[] Arguments => null;

        public Action CommandAction { get; set; }

        public object[] Indexes => new object[] 
        {
            (new CommandIndex()),
        };

        public async Task<string> ExecuteCommand(Argument[] args, object[] indexes) 
        {
            if (indexes[0] is CommandIndex commandIndex) 
            {
                CommandAction.Invoke();
                return $"{commandIndex.Name}: {commandIndex.Value}";
            }
            await ServerLogger.Log<ErrorPrefix>("This command was found", TimeFormat.HALF);
            return null; 
        }
    }
}
