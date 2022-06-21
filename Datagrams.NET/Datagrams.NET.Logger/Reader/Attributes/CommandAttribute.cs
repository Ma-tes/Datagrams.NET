
namespace DatagramsNet.Datagrams.NET.Logger.Reader.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class CommandAttribute : Attribute
    {
        public string Command { get; set; } 

        public string HelpText { get; set; }

        public CommandAttribute(string command, string help) 
        {
            Command = command;
            HelpText = help;
        }
    }
}
