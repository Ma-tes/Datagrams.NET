using DatagramsNet.Interfaces;

namespace DatagramsNet.Prefixes
{
    public abstract class StandardPrefix : IPrefix 
    {
        public abstract string Name { get; }

        public abstract ConsoleColor Color { get; }

        public virtual async Task WritePrefixAsync() 
        {
            Console.ForegroundColor = Color;
            await Console.Out.WriteAsync($"[{Name}]: ");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
