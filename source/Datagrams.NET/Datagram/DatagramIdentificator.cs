namespace DatagramsNet.Datagram
{

    internal sealed class DatagramIdentificator
    {
        public ReadOnlyMemory<byte[]> DatagramData { get; set; }

        public DatagramIdentificator() { }

        public DatagramIdentificator(ReadOnlyMemory<byte[]> binaryData)
        {
            DatagramData = binaryData;
        }

        public byte[] SerializeDatagram()
        {
            var datagramBytes = new List<byte>();
            for (int i = 0; i < DatagramData.Length; i++)
            {
                for (int j = 0; j < DatagramData.Span[i].Length; j++)
                {
                    var data = DatagramData.Span[i][j];
                    datagramBytes.Add(data);
                }
            }
            return datagramBytes.ToArray();
        }
    }
}
