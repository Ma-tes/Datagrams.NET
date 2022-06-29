namespace DatagramsNet.Datagram
{
    public abstract class DatagramHolder
    {
        protected abstract TimeSpan datagramHoldTime { get; }

        protected List<byte[]> LastData = new();

        public List<byte[]> CurrentData { get; protected set; }
    }
}
