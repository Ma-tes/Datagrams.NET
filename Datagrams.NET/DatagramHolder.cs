using System.Diagnostics;

namespace DatagramsNet
{
    public struct DatagramInfo 
    {
        public int FieldCount { get; set; }

        public int DatagramInfoId { get; init; }

        public List<byte[]> bytes = new();

        public Stopwatch DatagramPending = new();
    }

    public abstract class DatagramHolder
    {
        protected abstract TimeSpan datagramHoldTime { get; }

        protected abstract int maxDatagramId { get; }

        private List<DatagramInfo> pendingsDatagrams = new();

        protected List<byte[]> LastData = new();

        public List<byte[]> CurrentData { get; protected set; }

        public void SepareteDataAsync(byte[] data) 
        {
            int subDatagramId = 0;
            for (int i = 0; i < 4; i++)
            {
                subDatagramId = subDatagramId + data[i];
            }

            Span<byte> newData = new Span<byte>(data);
            Memory<byte[]> memory = new Memory<byte[]>();
            newData = newData.Slice(4, data.Length - 4);
            memory.Span.Fill(newData.ToArray());

            object idData = DatagramHelper.ReadDatagram(memory);
            var finalizedObject = pendingsDatagrams.FirstOrDefault(d => d.bytes.Count == d.FieldCount);

            if (finalizedObject.FieldCount > 0 && finalizedObject.FieldCount == finalizedObject.bytes.Count) 
            {
                int datagramIndex = pendingsDatagrams.IndexOf(finalizedObject);
                CurrentData = finalizedObject.bytes;
                pendingsDatagrams.Remove(finalizedObject);
                LastData.RemoveAt(datagramIndex);
            }

            var pendingDatagram = pendingsDatagrams.LastOrDefault(d => d.DatagramInfoId == subDatagramId);

            if (pendingDatagram.DatagramInfoId > 0)
            {
                pendingDatagram.bytes.Add(newData.ToArray());
                LastData.Add(data);
                if (idData is not null)
                {
                    pendingDatagram.FieldCount = GetFields(idData);
                    SwitchBytes(pendingDatagram.bytes[0], pendingDatagram.bytes[pendingDatagram.bytes.Count - 1]);
                }
            }
            else 
            {
                var datagramInfo = new DatagramInfo()
                {
                    DatagramInfoId = subDatagramId,
                };
                if (idData is not null)
                    datagramInfo.FieldCount = GetFields(idData);
                datagramInfo.bytes.Add(newData.ToArray());
                LastData.Add(data);
                pendingsDatagrams.Add(datagramInfo);
            }
        }

        protected virtual int GetFields(object @object) 
        {
            return @object.GetType().GetFields().Length;
        }

        private void SwitchBytes(byte[] firstByte, byte[] secondByte) 
        {
            var oldFirstByte = firstByte;
            firstByte = secondByte;
            secondByte = oldFirstByte;
        }

        private bool CheckDatagramLength(DatagramInfo datagramInfo) => datagramInfo.bytes.Count == (datagramInfo.FieldCount);
    }
}
