using DatagramsNet.Interfaces;

namespace DatagramsNet.Logging
{
    public struct Message
    {
        public string SingleMessage { get; set; }

        public IPrefix Prefix { get; set; }
    }
}
