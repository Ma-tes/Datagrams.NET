using DatagramsNet.Logging.Reading.Models;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class FileArgument : IArgument<string, FileArgument>
    {
        public string Name => "@file";

        public string Value { get; set; }

        public FileArgument GetArgument(string command, char separator, int index)
        {
            var values = command.Split(separator);
            var indexValue = values.Length - 1 >= index + 1 ? values[index + 1] : null;

            if (indexValue is not null)
            {
                if (File.Exists(indexValue))
                    return new FileArgument() { Value = indexValue };
                else
                    Task.Run(async () => await ServerLogger.LogAsync<ErrorPrefix>("This path was not found"));
            }
            return null;
        }
    }
}
