using DatagramsNet.Logging.Reading.Models;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class OptionsArgument : IArgument<Option[], OptionsArgument>
    {
        public string Name => "@argument";
        public Option[] Value { get; set; }

        public ICommand Command { get; init; }

        public OptionsArgument GetArgument(string command, char separator, int index)
        {
            var values = command.Split(separator);
            var indexValue = values.Length - 1 >= index + 1 ? values[index + 1] : null;
            var properArguments = Array.Empty<Option>();
            if (indexValue is not null)
                properArguments = GetProperArguments(Command.Options, indexValue.ToCharArray()).ToArray();

            return new OptionsArgument() { Command = Command, Value = properArguments };
        }

        private static IEnumerable<Option> GetProperArguments(Option[] baseArguments, char[] argumentsChar)
        {
            for (int i = 0; i < argumentsChar.Length; i++)
            {
                int index = 0;
                for (int j = 0; j < baseArguments.Length; j++)
                {
                    if (argumentsChar[i] == baseArguments[j].Character)
                    {
                        index++;
                        yield return new Option(argumentsChar[i]);
                    }
                }
                if (index == 0)
                    _ = ServerLogger.LogAsync<ErrorPrefix>($"Argument '{argumentsChar[i]}' was not found.");
            }
        }
    }
}
