
namespace DatagramsNet.Interfaces
{
    public interface IPrefix
    {
        public string Name { get; }
        public ConsoleColor Color { get; }

        public Task WritePrefixAsync()
        {
            return Task.CompletedTask;
        }
    }
}
