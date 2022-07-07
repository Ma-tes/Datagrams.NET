using DatagramsNet.Datagrams.NET.Logger.Reader.Attributes;
using DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces;
using System.Reflection;

namespace DatagramsNet.Datagrams.NET.Logger.Reader.Indexes
{
    internal sealed class CommandIndex : IIndex<string, CommandIndex>
    {
        private readonly Type commandAttributeType = typeof(CommandAttribute);

        private const char commandSeparator = '\n';

        public string Name => "@Command";

        public string Value { get; set; }

        public CommandIndex GetIndex(string command, char separator, int index) 
        {
            var values = command.Split(separator);
            var indexValue = (values.Length - 1) >= (index + 1) ? values[index + 1] : null;

            if (indexValue is not null) 
            {
                var commandAttributes = GetCommandAttributes();
                CommandAttribute commandAttribute = commandAttributes.FirstOrDefault(n => n.Command == indexValue);
                if (commandAttribute is not null)
                    return new CommandIndex() { Value = commandAttribute.HelpText };
                else
                    return null;
            }
            var commandsHelpText = GetCommandsHelpText();
            return new CommandIndex() { Value = string.Join(commandSeparator, commandsHelpText) };
        }

        private IEnumerable<string> GetCommandsHelpText() 
        {
            var commandAttributes = GetCommandAttributes();
            for (int i = 0; i < commandAttributes.Length; i++)
            {
                yield return commandAttributes[i].HelpText;
            }  
        }

        private CommandAttribute[] GetCommandAttributes() 
        {
            var commandAssemblies = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.GetCustomAttributes(commandAttributeType, true).Length > 0).ToArray();
            var commandAttributes = new CommandAttribute[commandAssemblies.Length];

            for (int i = 0; i < commandAssemblies.Length; i++) 
            {
                commandAttributes[i] = (CommandAttribute)commandAssemblies[i].GetCustomAttribute(commandAttributeType);
            }
            return commandAttributes;
        }
    }
}
