
namespace DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces
{
    public struct Argument 
    {
        public char Character { get; set; }

        public Argument(char character) => Character = character;
    }

    public interface ICommand
    {
        public Argument[] Arguments { get; }

        public object[] Indexes { get; }

        public Task<string> ExecuteCommand(Argument[] args, object[] indexes);
    }
}
