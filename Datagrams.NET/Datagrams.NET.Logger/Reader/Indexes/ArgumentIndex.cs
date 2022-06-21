using DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces;
using DatagramsNet.Datagrams.NET.Prefixes;

namespace DatagramsNet.Datagrams.NET.Logger.Reader.Indexes
{
    internal sealed class ArgumentIndex : IIndex<Argument[], ArgumentIndex>
    {
        public string Name => "@argument";

        public ICommand Command { get; set; }

        public Argument[] Value { get; set; }

        public ArgumentIndex GetIndex(string command, char separator, int index) 
        {
            var values = command.Split(separator);
            var indexValue = (values.Length - 1) >= (index + 1) ? values[index + 1] : null;
            List<Argument> properArguments = new List<Argument>();
            if (indexValue is not null)
                properArguments = GetProperArguments(Command.Arguments, indexValue.ToCharArray()).ToList();

            return new ArgumentIndex() { Command = this.Command, Value = properArguments.ToArray()};
        }

        private IEnumerable<Argument> GetProperArguments(Argument[] baseArguments, char[] argumentsChar) 
        {
            for (int i = 0; i < argumentsChar.Length; i++)
            {
                int index = 0;
                for (int j = 0; j < baseArguments.Length; j++)
                {
                    if (argumentsChar[i] == baseArguments[j].Character)
                    {
                        index++;
                        yield return new Argument() {Character = argumentsChar[i] };
                    }
                }
                if(index == 0)
                    ServerLogger.Log<ErrorPrefix>($"Sorry but argument '{argumentsChar[i]}' was not found");
            }
        }
    }
}
