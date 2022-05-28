
namespace DatagramsNet.Interfaces
{
    public interface IPrefix
    {
        public string Name { get; }

        public ConsoleColor Color { get; }

        public async Task WritePrefixAsync() { }
    }
}
