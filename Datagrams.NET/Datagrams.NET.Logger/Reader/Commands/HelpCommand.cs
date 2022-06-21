using DatagramsNet.Datagrams.NET.Logger.Reader.Attributes;
using DatagramsNet.Datagrams.NET.Logger.Reader.Indexes;
using DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces;
using DatagramsNet.Datagrams.NET.Prefixes;

namespace DatagramsNet.Datagrams.NET.Logger.Reader.Commands
{
    [Command("Help", "Help: [Command]")]
    internal sealed class HelpCommand : ICommand
    {
        public Argument[] Arguments => null;

        public object[] Indexes => new object[] 
        {
            (new CommandIndex()),
        };

        public async Task<string> ExecuteCommand(Argument[] args, object[] indexes) 
        {
            if (indexes[0] is CommandIndex commandIndex) 
            {
                return $"{commandIndex.Name}: {commandIndex.Value}";
            }
            await ServerLogger.Log<ErrorPrefix>("This command was found", TimeFormat.HALF);
            return null; 
        }
    }
}
