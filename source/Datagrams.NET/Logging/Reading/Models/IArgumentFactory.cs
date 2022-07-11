using System.Diagnostics.CodeAnalysis;

namespace DatagramsNet.Logging.Reading.Models
{
    public interface IArgumentFactory<TArgument> : IFactory
    {
        public bool TryCreate(string arg, [NotNullWhen(true)] out TArgument? argument);
    }
}
