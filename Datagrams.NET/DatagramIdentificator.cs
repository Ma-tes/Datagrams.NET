
namespace DatagramsNet
{

    internal sealed class DatagramIdentificator
    {
        private List<byte[]> datagramData;

        public DatagramIdentificator(List<byte[]> binaryData) 
        {
            datagramData = binaryData; 
        }

        public IEnumerable<byte> SerializeDatagram() 
        {
            for (int i = 0; i < datagramData.Count; i++)
            {
                for (int j = 0; j < datagramData[i].Length; j++)
                {
                    var data = datagramData[i][j];
                    yield return data;
                }
            }
        }

        public IEnumerable<byte[]> DeserializeDatagram(byte[] subData, int[] subBytesLength) 
        {
            Memory<byte> bytes = subData;
            int lastIndex = 0;
            for (int i = 0; i < subBytesLength.Length; i++)
            {
                var dataSnippet = bytes.Slice(lastIndex, subBytesLength[i]);
                lastIndex += subBytesLength[i];
                yield return dataSnippet.ToArray();
            }
        }
    }
}
