using DatagramsNet.Interfaces;

namespace DatagramsNet.Datagrams.NET.Logger
{
    public struct Message 
    {
        public string SingleMessage { get; set; }

        public IPrefix Prefix { get; set; }
    }
}
