using DatagramsNet.Interfaces;

namespace DatagramsNet.Logging
{
    internal readonly struct Message
    {
        public string Content { get; init; }
        public IPrefix? Prefix { get; init; }
    }
}
