using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Models;
using System.Reflection;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class CommandArgument : IArgument<string, CommandArgument>
    {
        private const char commandSeparator = '\n';

        public string Name => "@Command";

        public string Value { get; set; }

        public CommandArgument GetArgument(string command, char separator, int index)
        {
            var values = command.Split(separator);
            var indexValue = values.Length - 1 >= index + 1 ? values[index + 1] : null;

            if (indexValue is not null)
            {
                var commandAttributes = GetCommandAttributes();
                CommandAttribute commandAttribute = commandAttributes.FirstOrDefault(n => n.Command == indexValue);
                if (commandAttribute is not null)
                    return new CommandArgument() { Value = commandAttribute.HelpText };
                else
                    return null;
            }
            var commandsHelpText = GetCommandsHelpText();
            return new CommandArgument() { Value = string.Join(commandSeparator, commandsHelpText) };
        }

        private IEnumerable<string> GetCommandsHelpText()
        {
            var commandAttributes = GetCommandAttributes();
            for (int i = 0; i < commandAttributes.Length; i++)
            {
                yield return commandAttributes[i].HelpText;
            }
        }

        private static CommandAttribute[] GetCommandAttributes()
        {
            var commandAssemblies = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.GetCustomAttributes<CommandAttribute>(true).Any()).ToArray();
            var commandAttributes = new CommandAttribute[commandAssemblies.Length];

            for (int i = 0; i < commandAssemblies.Length; i++)
            {
                commandAttributes[i] = commandAssemblies[i].GetCustomAttribute<CommandAttribute>();
            }
            return commandAttributes;
        }
    }
}
