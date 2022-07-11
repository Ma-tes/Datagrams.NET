using System.Diagnostics.CodeAnalysis;

namespace DatagramsNet.Logging.Reading.Models
{
    public abstract class ArgumentFactory<TArgument> : IArgumentFactory<TArgument>, IFactory
    {
        public abstract string Name { get; }

        public object? Create(string arg)
        {
            return TryCreate(arg, out var result) ? result : null;
        }

        public abstract bool TryCreate(string arg, [NotNullWhen(true)] out TArgument? argument);
    }
}
