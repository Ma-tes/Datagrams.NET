using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Reflection;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Datagram
{
    public static class DatagramHelper
    {
        private static readonly MethodInfo read = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read))!;

        public static async Task SendDatagramAsync(Func<byte[], Task> sendAction, ReadOnlyMemory<byte[]> data)
        {
            var newSubData = Task.Run(() => new DatagramIdentificator(data).SerializeDatagram());
            await sendAction(newSubData.Result.ToArray());
        }

        public static Memory<byte[]> WriteDatagram<T>(T datagram) where T : IDatagram
        {
            var datagramList = new List<byte[]>();
            var customProperties = datagram.GetType().GetFields();
            for (int i = 0; i < customProperties.Length; i++)
            {
                var value = customProperties[i].GetValue(datagram);

                if (value is not null)
                {
                    datagramList.Add(BinaryHelper.Write(value));
                }
            }
            return datagramList.ToArray();
        }

        public static object? ReadDatagram(Memory<byte[]> datagram)
        {
            int datagramId = BinaryHelper.Read<int>(datagram.Span[0]);
            Type? datagramType = GetBaseDatagramType(datagramId, typeof(PacketAttribute));
            if (datagramType is null)
                return null;
            return SetDatagramData(datagramType, datagram);
        }

        private static object SetDatagramData(Type datagramType, Memory<byte[]> data)
        {
            var datagram = Activator.CreateInstance(datagramType)!;
            FieldInfo[] fields = datagram.GetType().GetFields();
            for (int i = 1; i < data.Length; i++)
            {
                Type fieldType = fields[i].FieldType;
                var fieldValue = read.MakeGenericMethod(fieldType).Invoke(null, new[] { data.Span[i] });
                fields[i].SetValue(datagram, fieldValue);
            }
            return datagram;
        }

        public static Type? GetBaseDatagramType(int id, Type classAttributeType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes(classAttributeType, true).Length > 0)).ToArray();
            for (int i = 0; i < assemblies.Length; i++)
            {
                FieldInfo[] properties = assemblies[i].GetFields();
                object? fieldValue = properties[0].GetValue(Activator.CreateInstance(assemblies[i])!);
                if (fieldValue is int datagramId && datagramId == id)
                    return assemblies[i];
            }
            return null;
        }
    }
}
