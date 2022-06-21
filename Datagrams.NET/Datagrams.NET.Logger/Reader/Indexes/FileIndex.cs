using DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces;
using DatagramsNet.Datagrams.NET.Prefixes;

namespace DatagramsNet.Datagrams.NET.Logger.Reader.Indexes
{
    internal sealed class FileIndex : IIndex<string, FileIndex>
    {
        public string Name => "@file";

        public string Value { get; set; }

        public FileIndex GetIndex(string command, char separator, int index) 
        {
            var values = command.Split(separator);
            var indexValue = (values.Length - 1) >= (index + 1) ? values[index + 1] : null;

            if (indexValue is not null) 
            {
                if (File.Exists(indexValue))
                    return new FileIndex() { Value = indexValue };
                else 
                    Task.Run(async () => await ServerLogger.Log<ErrorPrefix>("This path was not found"));
            }
            return null;
        }
    } 
}
